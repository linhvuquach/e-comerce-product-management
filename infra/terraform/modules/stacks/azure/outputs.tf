# =========================================================
# Azure Stack outputs — identical names to stacks/aws/outputs.tf
# Helm / deploy.sh reads these via `terraform output -raw <key>`
# =========================================================

output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  value = module.aks.cluster_name
}

output "aks_cluster_id" {
  value = module.aks.cluster_id
}

output "oidc_issuer_url" {
  value = module.aks.oidc_issuer_url
}

output "acr_login_server" {
  value = module.registry.login_server
}

output "acr_name" {
  value = module.registry.registry_name
}

output "db_host" {
  value = module.postgresql.fqdn
}

output "db_name" {
  value = module.postgresql.db_name
}

output "db_user" {
  value = module.postgresql.db_user
}

output "redis_host" {
  value = module.redis.hostname
}

output "redis_ssl_port" {
  value = module.redis.ssl_port
}

output "blob_endpoint" {
  value = module.storage.primary_blob_endpoint
}

output "cdn_endpoint_url" {
  value = module.storage.cdn_endpoint_url
}

output "blob_container_name" {
  value = module.storage.container_name
}

output "key_vault_uri" {
  value = module.keyvault.key_vault_uri
}

output "key_vault_name" {
  value = module.keyvault.key_vault_name
}

output "api_identity_client_id" {
  value = module.keyvault.api_identity_client_id
}

output "log_analytics_workspace_id" {
  value = module.monitoring.log_analytics_workspace_id
}
