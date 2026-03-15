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

variable "sku" {
  description = "ACR SKU: Basic | Standard | Premium"
  type        = string
  default     = "Standard"
}

variable "geo_replication_locations" {
  description = "Additional regions for geo-replication (Premium SKU only)"
  type        = list(string)
  default     = []
}

variable "subnet_infra_id" {
  description = "Infra subnet ID for private endpoint"
  type        = string
}

variable "private_dns_zone_acr_id" {
  description = "Private DNS zone ID for ACR"
  type        = string
}

variable "tags" {
  type    = map(string)
  default = {}
}
