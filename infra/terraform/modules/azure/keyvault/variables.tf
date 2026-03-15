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

variable "subnet_infra_id" {
  type = string
}

variable "private_dns_zone_keyvault_id" {
  type = string
}

variable "aks_oidc_issuer_url" {
  description = "AKS OIDC issuer URL (for Workload Identity federated credential)"
  type        = string
}

variable "secrets" {
  description = "Map of secret name → value"
  type        = map(string)
  sensitive   = true
  default     = {}
}

variable "tags" {
  type    = map(string)
  default = {}
}
