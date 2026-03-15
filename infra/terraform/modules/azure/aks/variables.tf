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

variable "kubernetes_version" {
  description = "Kubernetes version (null = use latest stable)"
  type        = string
  default     = null
}

variable "subnet_aks_id" {
  description = "AKS node subnet ID"
  type        = string
}

variable "system_node_vm_size" {
  description = "VM size for system node pool"
  type        = string
  default     = "Standard_B4ms"
}

variable "user_node_vm_size" {
  description = "VM size for user (workload) node pool"
  type        = string
  default     = "Standard_D4s_v3"
}

variable "user_node_min_count" {
  description = "Minimum nodes in user pool (autoscaler)"
  type        = number
  default     = 1
}

variable "user_node_max_count" {
  description = "Maximum nodes in user pool (autoscaler)"
  type        = number
  default     = 3
}

variable "acr_id" {
  description = "ACR resource ID — grants AKS kubelet identity AcrPull"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID for Container Insights"
  type        = string
}

variable "tags" {
  type    = map(string)
  default = {}
}
