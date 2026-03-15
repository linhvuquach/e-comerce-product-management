# =========================================================
# AWS Stack — STUB
# Mirrors the Azure stack interface.
# Implement modules/aws/* to activate.
# =========================================================

# TODO: Uncomment and implement when switching to AWS

# module "networking" {
#   source = "../../aws/networking"
#   # VPC, public/private subnets, security groups, Route53 private zones
# }

# module "registry" {
#   source = "../../aws/registry"
#   # ECR — image repository per service
# }

# module "eks" {
#   source = "../../aws/eks"
#   # EKS cluster with managed node groups, IRSA (IAM Roles for Service Accounts)
#   # IRSA replaces Azure Workload Identity — same concept, different implementation
# }

# module "postgresql" {
#   source = "../../aws/postgresql"
#   # RDS PostgreSQL with Multi-AZ (equivalent to Azure HA), private subnet
# }

# module "redis" {
#   source = "../../aws/redis"
#   # ElastiCache Redis, private subnet, auth token in Secrets Manager
# }

# module "storage" {
#   source = "../../aws/storage"
#   # S3 bucket + CloudFront distribution (equivalent to Azure Blob + CDN)
# }

# module "secrets" {
#   source = "../../aws/secrets"
#   # AWS Secrets Manager + IRSA for pod access (no stored credentials)
#   # ExternalSecret manifest in Helm chart uses SecretStore pointing to Secrets Manager
# }
