data "azurerm_client_config" "current" {}

locals {
  name = "${var.prefix}-${var.environment}-kv"
}

resource "azurerm_key_vault" "main" {
  name                          = local.name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  tenant_id                     = data.azurerm_client_config.current.tenant_id
  sku_name                      = "standard"
  soft_delete_retention_days    = 7
  purge_protection_enabled      = var.environment == "prod" ? true : false
  public_network_access_enabled = false
  tags                          = var.tags

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }
}

# Private endpoint
resource "azurerm_private_endpoint" "keyvault" {
  name                = "${local.name}-pe"
  resource_group_name = var.resource_group_name
  location            = var.location
  subnet_id           = var.subnet_infra_id
  tags                = var.tags

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_key_vault.main.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }

  private_dns_zone_group {
    name                 = "kv-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_keyvault_id]
  }
}

# Managed Identity used by the API pod (Workload Identity)
resource "azurerm_user_assigned_identity" "api" {
  name                = "${var.prefix}-${var.environment}-api-identity"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags
}

# Grant the API identity access to read secrets
resource "azurerm_key_vault_access_policy" "api" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.api.principal_id

  secret_permissions = ["Get", "List"]
}

# Grant Terraform (current client) access to write secrets during apply
resource "azurerm_key_vault_access_policy" "terraform" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
}

# Store all secrets passed in from other modules
resource "azurerm_key_vault_secret" "secrets" {
  for_each     = var.secrets
  name         = each.key
  value        = each.value
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.terraform]
}

# Federated identity credential: AKS pod (api service account) → Key Vault identity
# This allows the API pod to authenticate to Key Vault without any stored credentials.
resource "azurerm_federated_identity_credential" "api" {
  name                = "${var.prefix}-${var.environment}-api-federated"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.api.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.aks_oidc_issuer_url
  subject             = "system:serviceaccount:product-management:api"
}
