locals {
  name = "${var.prefix}-${var.environment}"
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "${local.name}-law"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days
  tags                = var.tags
}

# AKS diagnostic settings → Log Analytics
resource "azurerm_monitor_diagnostic_setting" "aks" {
  name               = "${local.name}-aks-diag"
  target_resource_id = var.aks_cluster_id

  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  enabled_log { category = "kube-apiserver" }
  enabled_log { category = "kube-controller-manager" }
  enabled_log { category = "kube-scheduler" }
  enabled_log { category = "kube-audit" }
  enabled_log { category = "kube-audit-admin" }
  enabled_log { category = "guard" }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
