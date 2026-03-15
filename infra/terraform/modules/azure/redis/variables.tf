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

variable "private_dns_zone_redis_id" {
  type = string
}

variable "sku_name" {
  description = "Redis SKU: Basic | Standard | Premium"
  type        = string
  default     = "Basic"
}

variable "family" {
  description = "Redis family: C (Basic/Standard) | P (Premium)"
  type        = string
  default     = "C"
}

variable "capacity" {
  description = "Redis cache size (0=250MB, 1=1GB, 2=6GB, ...)"
  type        = number
  default     = 0
}

variable "enable_non_ssl_port" {
  description = "Allow plaintext port 6379 (never in prod)"
  type        = bool
  default     = false
}

variable "tags" {
  type    = map(string)
  default = {}
}
