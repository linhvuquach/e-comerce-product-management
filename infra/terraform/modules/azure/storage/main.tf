locals {
  # Storage account names: 3-24 lowercase alphanumeric only
  sa_name = "${var.prefix}${var.environment}blob"
  name    = "${var.prefix}-${var.environment}"
}

resource "azurerm_storage_account" "main" {
  name                            = local.sa_name
  resource_group_name             = var.resource_group_name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = var.environment == "prod" ? "ZRS" : "LRS"
  allow_nested_items_to_be_public = false
  min_tls_version                 = "TLS1_2"
  https_traffic_only_enabled      = true
  tags                            = var.tags

  blob_properties {
    versioning_enabled       = true
    change_feed_enabled      = true
    delete_retention_policy {
      days = 7
    }
    container_delete_retention_policy {
      days = 7
    }
  }
}

resource "azurerm_storage_container" "images" {
  name                  = var.container_name
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

# Grant the API workload identity read/write access to blobs
resource "azurerm_role_assignment" "blob_contributor" {
  count                = var.workload_identity_principal_id != "" ? 1 : 0
  scope                = azurerm_storage_account.main.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.workload_identity_principal_id
}

# CDN profile
resource "azurerm_cdn_profile" "main" {
  name                = "${local.name}-cdn"
  resource_group_name = var.resource_group_name
  location            = "global"
  sku                 = var.cdn_sku
  tags                = var.tags
}

# CDN endpoint pointing to Blob storage origin
resource "azurerm_cdn_endpoint" "images" {
  name                = "${local.name}-images"
  profile_name        = azurerm_cdn_profile.main.name
  resource_group_name = var.resource_group_name
  location            = "global"
  tags                = var.tags

  origin_host_header = azurerm_storage_account.main.primary_blob_host

  origin {
    name      = "blob-origin"
    host_name = azurerm_storage_account.main.primary_blob_host
  }

  delivery_rule {
    name  = "EnforceHTTPS"
    order = 1

    request_scheme_condition {
      operator     = "Equal"
      match_values = ["HTTP"]
    }

    url_redirect_action {
      redirect_type = "PermanentRedirect"
      protocol      = "Https"
    }
  }
}
