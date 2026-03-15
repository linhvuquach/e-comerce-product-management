locals {
  name = "${var.prefix}-${var.environment}-aks"
}

resource "azurerm_kubernetes_cluster" "main" {
  name                = local.name
  resource_group_name = var.resource_group_name
  location            = var.location
  dns_prefix          = "${var.prefix}-${var.environment}"
  kubernetes_version  = var.kubernetes_version
  tags                = var.tags

  # System node pool — runs kube-system, ArgoCD, cert-manager etc.
  default_node_pool {
    name                        = "system"
    vm_size                     = var.system_node_vm_size
    node_count                  = 1
    vnet_subnet_id              = var.subnet_aks_id
    only_critical_addons_enabled = true
    os_disk_size_gb             = 60
    os_disk_type                = "Ephemeral"

    upgrade_settings {
      max_surge = "10%"
    }
  }

  # Workload Identity + OIDC issuer (required for External Secrets Operator)
  oidc_issuer_enabled       = true
  workload_identity_enabled = true

  # Managed identity for the cluster control plane
  identity {
    type = "SystemAssigned"
  }

  # Azure CNI — pods get VNet IPs (required for private endpoint connectivity)
  network_profile {
    network_plugin     = "azure"
    network_policy     = "azure"
    load_balancer_sku  = "standard"
    outbound_type      = "loadBalancer"
  }

  # Container Insights
  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  # Azure AD RBAC (recommended over local accounts)
  azure_active_directory_role_based_access_control {
    azure_rbac_enabled = true
  }

  # Disable local admin account — use AAD RBAC only
  local_account_disabled = true
}

# User node pool — runs application workloads with autoscaler
resource "azurerm_kubernetes_cluster_node_pool" "user" {
  name                  = "user"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.user_node_vm_size
  vnet_subnet_id        = var.subnet_aks_id
  os_disk_size_gb       = 128
  os_disk_type          = "Managed"
  mode                  = "User"

  auto_scaling_enabled = true
  min_count            = var.user_node_min_count
  max_count            = var.user_node_max_count

  node_labels = {
    "workload" = "application"
  }

  upgrade_settings {
    max_surge = "33%"
  }

  tags = var.tags
}

# Grant AKS kubelet identity AcrPull on ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
}
