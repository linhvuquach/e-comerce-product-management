module "stack" {
  source = "../../modules/stacks/azure"
  # To switch to AWS: source = "../../modules/stacks/aws"

  environment = "dev"
  location    = var.location
  prefix      = var.prefix
  tags        = var.tags

  # Networking (defaults are fine for dev)

  # AKS — single small node
  system_node_vm_size = "Standard_B4ms"
  user_node_vm_size   = "Standard_B4ms"
  user_node_min_count = 1
  user_node_max_count = 2

  # ACR — Basic (no private endpoint, faster + cheaper for dev)
  acr_sku = "Basic"

  # PostgreSQL — smallest burstable tier
  postgres_sku_name = "B_Standard_B1ms"
  postgres_ha_enabled = false

  # Redis — Basic C0
  redis_sku_name = "Basic"
  redis_family   = "C"
  redis_capacity = 0

  # CDN
  cdn_sku = "Standard_Microsoft"

  # Auth (disabled in dev — set to real values when enabling)
  jwt_authority = var.jwt_authority
  jwt_audience  = var.jwt_audience

  # Monitoring — short retention to save cost
  log_retention_days = 30
}
