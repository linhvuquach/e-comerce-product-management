# =========================================================
# AWS Stack outputs — MUST stay identical to stacks/azure/outputs.tf
# deploy.sh and Helm use these output names regardless of provider.
# =========================================================

# TODO: Uncomment and wire to module outputs when implementing AWS stack

# output "resource_group_name"      { value = module.networking.vpc_name }  # closest equivalent
# output "aks_cluster_name"         { value = module.eks.cluster_name }
# output "aks_cluster_id"           { value = module.eks.cluster_id }
# output "oidc_issuer_url"          { value = module.eks.oidc_issuer_url }
# output "acr_login_server"         { value = module.registry.ecr_registry_url }
# output "acr_name"                 { value = module.registry.ecr_name }
# output "db_host"                  { value = module.postgresql.endpoint }
# output "db_name"                  { value = module.postgresql.db_name }
# output "db_user"                  { value = module.postgresql.db_user }
# output "redis_host"               { value = module.redis.primary_endpoint }
# output "redis_ssl_port"           { value = module.redis.port }
# output "blob_endpoint"            { value = module.storage.s3_bucket_domain }
# output "cdn_endpoint_url"         { value = module.storage.cloudfront_url }
# output "blob_container_name"      { value = module.storage.bucket_name }
# output "key_vault_uri"            { value = module.secrets.secrets_manager_arn }
# output "key_vault_name"           { value = module.secrets.secrets_manager_name }
# output "api_identity_client_id"   { value = module.secrets.irsa_role_arn }
# output "log_analytics_workspace_id" { value = module.eks.cloudwatch_log_group_arn }
