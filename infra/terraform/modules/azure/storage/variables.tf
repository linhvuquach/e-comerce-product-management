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

variable "container_name" {
  description = "Blob container for product images"
  type        = string
  default     = "products-images"
}

variable "cdn_sku" {
  description = "CDN profile SKU: Standard_Microsoft | Standard_Verizon | Premium_Verizon"
  type        = string
  default     = "Standard_Microsoft"
}

variable "workload_identity_principal_id" {
  description = "Principal ID of the AKS Workload Identity (API pod) to grant Storage Blob Data Contributor"
  type        = string
  default     = ""
}

variable "tags" {
  type    = map(string)
  default = {}
}
