module "stack" {
  source = "../../modules/stacks/azure"

  environment = "prod"
  location    = var.location
  prefix      = var.prefix
  tags        = var.tags

  # AKS — autoscaling 3–10 nodes, high-memory VMs
  system_node_vm_size = "Standard_D4s_v3"
  user_node_vm_size   = "Standard_D8s_v3"
  user_node_min_count = 3
  user_node_max_count = 10

  # ACR — Standard + geo-replication to secondary region
  acr_sku                       = "Standard"
  acr_geo_replication_locations = var.acr_secondary_locations

  # PostgreSQL — GP D4s_v3 + zone-redundant HA
  postgres_sku_name   = "GP_Standard_D4s_v3"
  postgres_storage_mb = 131072  # 128 GB
  postgres_ha_enabled = true

  # Redis — Standard C2 (6 GB)
  redis_sku_name = "Standard"
  redis_family   = "C"
  redis_capacity = 2

  jwt_authority      = var.jwt_authority
  jwt_audience       = var.jwt_audience
  log_retention_days = 90
}
