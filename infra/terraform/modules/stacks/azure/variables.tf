# =========================================================
# Azure Stack — unified interface
# Identical variable names to modules/stacks/aws/variables.tf
# Changing provider = swap source path in environments/*/main.tf
# =========================================================

variable "environment" {
  description = "Environment name: dev | staging | prod"
  type        = string
}

variable "location" {
  description = "Primary Azure region"
  type        = string
  default     = "australiaeast"
}

variable "prefix" {
  description = "Short resource name prefix"
  type        = string
  default     = "pm"
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

# ---------- AKS ----------
variable "kubernetes_version" {
  type    = string
  default = null
}

variable "system_node_vm_size" {
  type    = string
  default = "Standard_B4ms"
}

variable "user_node_vm_size" {
  type    = string
  default = "Standard_D4s_v3"
}

variable "user_node_min_count" {
  type    = number
  default = 1
}

variable "user_node_max_count" {
  type    = number
  default = 3
}

# ---------- ACR ----------
variable "acr_sku" {
  type    = string
  default = "Standard"
}

variable "acr_geo_replication_locations" {
  type    = list(string)
  default = []
}

# ---------- PostgreSQL ----------
variable "postgres_sku_name" {
  type    = string
  default = "B_Standard_B1ms"
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
  type    = bool
  default = false
}

variable "db_name" {
  type    = string
  default = "productmanagement"
}

variable "db_user" {
  type    = string
  default = "productmgmt"
}

# ---------- Redis ----------
variable "redis_sku_name" {
  type    = string
  default = "Basic"
}

variable "redis_family" {
  type    = string
  default = "C"
}

variable "redis_capacity" {
  type    = number
  default = 0
}

# ---------- Storage / CDN ----------
variable "blob_container_name" {
  type    = string
  default = "products-images"
}

variable "cdn_sku" {
  type    = string
  default = "Standard_Microsoft"
}

# ---------- App secrets ----------
variable "jwt_authority" {
  description = "Azure Entra ID OIDC authority URL"
  type        = string
  default     = ""
}

variable "jwt_audience" {
  description = "JWT audience for the API"
  type        = string
  default     = "api://product-management"
}

# ---------- Monitoring ----------
variable "log_retention_days" {
  type    = number
  default = 30
}

# ---------- Common ----------
variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
