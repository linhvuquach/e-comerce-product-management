output "key_vault_id" {
  value = azurerm_key_vault.main.id
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "key_vault_name" {
  value = azurerm_key_vault.main.name
}

output "api_identity_client_id" {
  value = azurerm_user_assigned_identity.api.client_id
}

output "api_identity_principal_id" {
  value = azurerm_user_assigned_identity.api.principal_id
}

output "api_identity_id" {
  value = azurerm_user_assigned_identity.api.id
}
