locals {
  name = "${var.prefix}-${var.environment}-redis"
}

resource "azurerm_redis_cache" "main" {
  name                = local.name
  resource_group_name = var.resource_group_name
  location            = var.location
  capacity            = var.capacity
  family              = var.family
  sku_name            = var.sku_name
  enable_non_ssl_port = var.enable_non_ssl_port
  minimum_tls_version = "1.2"
  tags                = var.tags

  redis_configuration {
    # AOF persistence for Standard/Premium only
    aof_backup_enabled = var.sku_name == "Premium" ? true : false
  }
}

# Private endpoint (Standard/Premium; Basic doesn't support it)
resource "azurerm_private_endpoint" "redis" {
  count               = var.sku_name != "Basic" ? 1 : 0
  name                = "${local.name}-pe"
  resource_group_name = var.resource_group_name
  location            = var.location
  subnet_id           = var.subnet_data_id
  tags                = var.tags

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_redis_cache.main.id
    is_manual_connection           = false
    subresource_names              = ["redisCache"]
  }

  private_dns_zone_group {
    name                 = "redis-dns-group"
    private_dns_zone_ids = [var.private_dns_zone_redis_id]
  }
}
