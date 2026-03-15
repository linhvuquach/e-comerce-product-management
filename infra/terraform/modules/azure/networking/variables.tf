variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "environment" {
  description = "Environment name (dev | staging | prod)"
  type        = string
}

variable "prefix" {
  description = "Short resource name prefix (e.g. 'pm')"
  type        = string
  default     = "pm"
}

variable "vnet_address_space" {
  description = "Address space for the VNet"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "subnet_aks_cidr" {
  description = "CIDR for the AKS node subnet"
  type        = string
  default     = "10.0.1.0/24"
}

variable "subnet_data_cidr" {
  description = "CIDR for the data subnet (PostgreSQL, Redis private endpoints)"
  type        = string
  default     = "10.0.2.0/24"
}

variable "subnet_infra_cidr" {
  description = "CIDR for the infra subnet (ACR, Key Vault private endpoints)"
  type        = string
  default     = "10.0.3.0/24"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
