output "storage_account_id" {
  value = azurerm_storage_account.main.id
}

output "storage_account_name" {
  value = azurerm_storage_account.main.name
}

output "primary_blob_endpoint" {
  value = azurerm_storage_account.main.primary_blob_endpoint
}

output "container_name" {
  value = azurerm_storage_container.images.name
}

output "cdn_endpoint_hostname" {
  value = azurerm_cdn_endpoint.images.fqdn
}

output "cdn_endpoint_url" {
  value = "https://${azurerm_cdn_endpoint.images.fqdn}"
}

output "primary_access_key" {
  value     = azurerm_storage_account.main.primary_access_key
  sensitive = true
}
