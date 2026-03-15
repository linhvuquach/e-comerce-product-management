locals {
  name = "${var.prefix}${var.environment}acr"
}

resource "azurerm_container_registry" "main" {
  name                = local.name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = var.sku
  admin_enabled       = false # Workload Identity pull only
  tags                = var.tags

  dynamic "georeplications" {
    for_each = var.geo_replication_locations
    content {
      location                = georeplications.value
      zone_redundancy_enabled = true
    }
  }
}

# Private endpoint (Standard/Premium only — omit for Basic in dev)
resource "azurerm_private_endpoint" "acr" {
  count               = var.sku != "Basic" ? 1 : 0
  name                = "${local.name}-pe"
  resource_group_name = var.resource_group_name
  location            = var.location
  subnet_id           = var.subnet_infra_id
  tags                = var.tags

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_container_registry.main.id
    is_manual_connection           = false
    subresource_names              = ["registry"]
  }

  private_dns_zone_group {
    name                 = "acr-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_acr_id]
  }
}
