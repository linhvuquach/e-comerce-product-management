variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "environment" {
  type = string
}

variable "prefix" {
  type    = string
  default = "pm"
}

variable "subnet_data_id" {
  type = string
}

variable "private_dns_zone_postgres_id" {
  type = string
}

variable "sku_name" {
  description = "PostgreSQL Flexible Server SKU (e.g. B_Standard_B1ms, GP_Standard_D4s_v3)"
  type        = string
  default     = "B_Standard_B1ms"
}

variable "storage_mb" {
  description = "Storage size in MB"
  type        = number
  default     = 32768 # 32 GB
}

variable "postgres_version" {
  description = "PostgreSQL major version"
  type        = string
  default     = "17"
}

variable "high_availability_enabled" {
  description = "Enable zone-redundant HA (prod only)"
  type        = bool
  default     = false
}

variable "db_name" {
  description = "Application database name"
  type        = string
  default     = "productmanagement"
}

variable "db_user" {
  description = "Application database username"
  type        = string
  default     = "productmgmt"
}

variable "tags" {
  type    = map(string)
  default = {}
}
