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

variable "aks_cluster_id" {
  type = string
}

variable "log_retention_days" {
  description = "Log Analytics data retention in days"
  type        = number
  default     = 30
}

variable "tags" {
  type    = map(string)
  default = {}
}
