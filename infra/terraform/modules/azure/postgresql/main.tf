locals {
  name = "${var.prefix}-${var.environment}-pg"
}

resource "random_password" "db" {
  length           = 32
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                          = local.name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  version                       = var.postgres_version
  delegated_subnet_id           = var.subnet_data_id
  private_dns_zone_id           = var.private_dns_zone_postgres_id
  public_network_access_enabled = false
  administrator_login           = var.db_user
  administrator_password        = random_password.db.result
  sku_name                      = var.sku_name
  tags                          = var.tags

  storage_mb   = var.storage_mb
  storage_tier = var.sku_name == "B_Standard_B1ms" ? "P4" : "P30"

  dynamic "high_availability" {
    for_each = var.high_availability_enabled ? [1] : []
    content {
      mode                      = "ZoneRedundant"
      standby_availability_zone = "2"
    }
  }

  maintenance_window {
    day_of_week  = 0 # Sunday
    start_hour   = 2
    start_minute = 0
  }

  lifecycle {
    ignore_changes = [
      # Password managed via Key Vault rotation — don't drift on apply
      administrator_password,
      zone,
      high_availability[0].standby_availability_zone
    ]
  }
}

# Application database
resource "azurerm_postgresql_flexible_server_database" "app" {
  name      = var.db_name
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}

# Required extensions
resource "azurerm_postgresql_flexible_server_configuration" "pg_trgm" {
  name      = "azure.extensions"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "PG_TRGM,UNACCENT,UUID-OSSP"
}
