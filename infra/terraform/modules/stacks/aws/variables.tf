# =========================================================
# AWS Stack — interface CONTRACT
# Variable names MUST stay identical to stacks/azure/variables.tf
# Helm charts and deploy.sh work unchanged when switching providers.
# =========================================================

variable "environment" {
  type = string
}

variable "location" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-2"
}

variable "prefix" {
  type    = string
  default = "pm"
}

# ---------- Networking ----------
variable "vnet_address_space" {
  type    = list(string)
  default = ["10.0.0.0/16"]
}

variable "subnet_aks_cidr" {
  type    = string
  default = "10.0.1.0/24"
}

variable "subnet_data_cidr" {
  type    = string
  default = "10.0.2.0/24"
}

variable "subnet_infra_cidr" {
  type    = string
  default = "10.0.3.0/24"
}

# ---------- EKS (equivalent to AKS) ----------
variable "kubernetes_version" {
  type    = string
  default = null
}

variable "system_node_vm_size" {
  description = "AWS instance type for system node pool"
  type        = string
  default     = "t3.medium"
}

variable "user_node_vm_size" {
  description = "AWS instance type for user node pool"
  type        = string
  default     = "t3.xlarge"
}

variable "user_node_min_count" {
  type    = number
  default = 1
}

variable "user_node_max_count" {
  type    = number
  default = 3
}

# ---------- ECR (equivalent to ACR) ----------
variable "acr_sku" {
  description = "Unused on AWS — kept for interface parity"
  type        = string
  default     = "Standard"
}

variable "acr_geo_replication_locations" {
  type    = list(string)
  default = []
}

# ---------- RDS PostgreSQL ----------
variable "postgres_sku_name" {
  description = "AWS RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "postgres_storage_mb" {
  type    = number
  default = 32768
}

variable "postgres_version" {
  type    = string
  default = "17"
}

variable "postgres_ha_enabled" {
  description = "Enable Multi-AZ (equivalent to Azure zone-redundant HA)"
  type        = bool
  default     = false
}

variable "db_name" {
  type    = string
  default = "productmanagement"
}

variable "db_user" {
  type    = string
  default = "productmgmt"
}

# ---------- ElastiCache Redis ----------
variable "redis_sku_name" {
  description = "AWS ElastiCache node type"
  type        = string
  default     = "cache.t3.micro"
}

variable "redis_family" {
  description = "Unused on AWS — kept for interface parity"
  type        = string
  default     = "C"
}

variable "redis_capacity" {
  description = "Unused on AWS — kept for interface parity"
  type        = number
  default     = 0
}

# ---------- S3 + CloudFront ----------
variable "blob_container_name" {
  type    = string
  default = "products-images"
}

variable "cdn_sku" {
  description = "Unused on AWS — kept for interface parity"
  type        = string
  default     = "Standard_Microsoft"
}

# ---------- App secrets ----------
variable "jwt_authority" {
  type    = string
  default = ""
}

variable "jwt_audience" {
  type    = string
  default = "api://product-management"
}

# ---------- Monitoring ----------
variable "log_retention_days" {
  type    = number
  default = 30
}

# ---------- Common ----------
variable "tags" {
  type    = map(string)
  default = {}
}
