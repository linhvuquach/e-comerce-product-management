output "resource_group_name" {
  value = module.stack.resource_group_name
}

output "aks_cluster_name" {
  value = module.stack.aks_cluster_name
}

output "acr_login_server" {
  value = module.stack.acr_login_server
}

output "db_host" {
  value = module.stack.db_host
}

output "redis_host" {
  value = module.stack.redis_host
}

output "key_vault_uri" {
  value = module.stack.key_vault_uri
}

output "api_identity_client_id" {
  value = module.stack.api_identity_client_id
}

output "cdn_endpoint_url" {
  value = module.stack.cdn_endpoint_url
}
