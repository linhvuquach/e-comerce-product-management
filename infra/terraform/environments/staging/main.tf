module "stack" {
  source = "../../modules/stacks/azure"

  environment = "staging"
  location    = var.location
  prefix      = var.prefix
  tags        = var.tags

  # AKS — 2 nodes, Standard tier VMs
  system_node_vm_size = "Standard_D2s_v3"
  user_node_vm_size   = "Standard_D4s_v3"
  user_node_min_count = 2
  user_node_max_count = 4

  # ACR — Standard with private endpoint
  acr_sku = "Standard"

  # PostgreSQL — general purpose, no HA
  postgres_sku_name   = "GP_Standard_D2s_v3"
  postgres_storage_mb = 65536
  postgres_ha_enabled = false

  # Redis — Standard C1
  redis_sku_name = "Standard"
  redis_family   = "C"
  redis_capacity = 1

  jwt_authority      = var.jwt_authority
  jwt_audience       = var.jwt_audience
  log_retention_days = 60
}
