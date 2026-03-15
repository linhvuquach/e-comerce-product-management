locals {
  name = "${var.prefix}-${var.environment}"

  common_tags = merge(var.tags, {
    environment  = var.environment
    managed-by   = "terraform"
    project      = "ecommerce-product-management"
  })
}

# ---------- Resource Group ----------
resource "azurerm_resource_group" "main" {
  name     = "${local.name}-rg"
  location = var.location
  tags     = local.common_tags
}

# ---------- Networking ----------
module "networking" {
  source = "../../azure/networking"

  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  environment         = var.environment
  prefix              = var.prefix
  vnet_address_space  = var.vnet_address_space
  subnet_aks_cidr     = var.subnet_aks_cidr
  subnet_data_cidr    = var.subnet_data_cidr
  subnet_infra_cidr   = var.subnet_infra_cidr
  tags                = local.common_tags
}

# ---------- Monitoring (created early — AKS needs workspace ID) ----------
module "monitoring" {
  source = "../../azure/monitoring"

  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  environment         = var.environment
  prefix              = var.prefix
  # aks_cluster_id set after AKS module — passed via separate diagnostic resource
  aks_cluster_id      = module.aks.cluster_id
  log_retention_days  = var.log_retention_days
  tags                = local.common_tags

  depends_on = [module.aks]
}

# ---------- ACR ----------
module "registry" {
  source = "../../azure/registry"

  resource_group_name     = azurerm_resource_group.main.name
  location                = var.location
  environment             = var.environment
  prefix                  = var.prefix
  sku                     = var.acr_sku
  geo_replication_locations = var.acr_geo_replication_locations
  subnet_infra_id         = module.networking.subnet_infra_id
  private_dns_zone_acr_id = module.networking.private_dns_zone_acr_id
  tags                    = local.common_tags
}

# ---------- AKS ----------
module "aks" {
  source = "../../azure/aks"

  resource_group_name             = azurerm_resource_group.main.name
  location                        = var.location
  environment                     = var.environment
  prefix                          = var.prefix
  kubernetes_version              = var.kubernetes_version
  subnet_aks_id                   = module.networking.subnet_aks_id
  system_node_vm_size             = var.system_node_vm_size
  user_node_vm_size               = var.user_node_vm_size
  user_node_min_count             = var.user_node_min_count
  user_node_max_count             = var.user_node_max_count
  acr_id                          = module.registry.registry_id
  log_analytics_workspace_id      = module.monitoring.log_analytics_workspace_id
  tags                            = local.common_tags

  depends_on = [module.networking, module.registry, module.monitoring]
}

# ---------- PostgreSQL ----------
module "postgresql" {
  source = "../../azure/postgresql"

  resource_group_name           = azurerm_resource_group.main.name
  location                      = var.location
  environment                   = var.environment
  prefix                        = var.prefix
  subnet_data_id                = module.networking.subnet_data_id
  private_dns_zone_postgres_id  = module.networking.private_dns_zone_postgres_id
  sku_name                      = var.postgres_sku_name
  storage_mb                    = var.postgres_storage_mb
  postgres_version              = var.postgres_version
  high_availability_enabled     = var.postgres_ha_enabled
  db_name                       = var.db_name
  db_user                       = var.db_user
  tags                          = local.common_tags

  depends_on = [module.networking]
}

# ---------- Redis ----------
module "redis" {
  source = "../../azure/redis"

  resource_group_name      = azurerm_resource_group.main.name
  location                 = var.location
  environment              = var.environment
  prefix                   = var.prefix
  subnet_data_id           = module.networking.subnet_data_id
  private_dns_zone_redis_id = module.networking.private_dns_zone_redis_id
  sku_name                 = var.redis_sku_name
  family                   = var.redis_family
  capacity                 = var.redis_capacity
  tags                     = local.common_tags

  depends_on = [module.networking]
}

# ---------- Blob Storage + CDN ----------
module "storage" {
  source = "../../azure/storage"

  resource_group_name             = azurerm_resource_group.main.name
  location                        = var.location
  environment                     = var.environment
  prefix                          = var.prefix
  container_name                  = var.blob_container_name
  cdn_sku                         = var.cdn_sku
  workload_identity_principal_id  = module.keyvault.api_identity_principal_id
  tags                            = local.common_tags

  depends_on = [module.keyvault]
}

# ---------- Key Vault + Workload Identity ----------
module "keyvault" {
  source = "../../azure/keyvault"

  resource_group_name          = azurerm_resource_group.main.name
  location                     = var.location
  environment                  = var.environment
  prefix                       = var.prefix
  subnet_infra_id              = module.networking.subnet_infra_id
  private_dns_zone_keyvault_id = module.networking.private_dns_zone_keyvault_id
  aks_oidc_issuer_url          = module.aks.oidc_issuer_url
  tags                         = local.common_tags

  # Pass all sensitive values from other modules into Key Vault
  secrets = {
    "db-password"            = module.postgresql.db_password
    "db-connection-string"   = module.postgresql.connection_string
    "redis-auth-token"       = module.redis.primary_access_key
    "redis-connection-string" = module.redis.connection_string
    "blob-connection-string" = "DefaultEndpointsProtocol=https;AccountName=${module.storage.storage_account_name};AccountKey=${module.storage.primary_access_key};EndpointSuffix=core.windows.net"
    "jwt-authority"          = var.jwt_authority
    "jwt-audience"           = var.jwt_audience
  }

  depends_on = [module.aks, module.networking, module.postgresql, module.redis]
}
