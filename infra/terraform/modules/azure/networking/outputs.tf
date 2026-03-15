output "vnet_id" {
  description = "Virtual Network ID"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Virtual Network name"
  value       = azurerm_virtual_network.main.name
}

output "subnet_aks_id" {
  description = "AKS node subnet ID"
  value       = azurerm_subnet.aks.id
}

output "subnet_data_id" {
  description = "Data subnet ID (PostgreSQL, Redis private endpoints)"
  value       = azurerm_subnet.data.id
}

output "subnet_infra_id" {
  description = "Infra subnet ID (ACR, Key Vault private endpoints)"
  value       = azurerm_subnet.infra.id
}

output "private_dns_zone_postgres_id" {
  description = "Private DNS Zone ID for PostgreSQL"
  value       = azurerm_private_dns_zone.postgres.id
}

output "private_dns_zone_redis_id" {
  description = "Private DNS Zone ID for Redis"
  value       = azurerm_private_dns_zone.redis.id
}

output "private_dns_zone_keyvault_id" {
  description = "Private DNS Zone ID for Key Vault"
  value       = azurerm_private_dns_zone.keyvault.id
}

output "private_dns_zone_acr_id" {
  description = "Private DNS Zone ID for ACR"
  value       = azurerm_private_dns_zone.acr.id
}
