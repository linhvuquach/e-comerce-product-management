output "server_id" {
  value = azurerm_postgresql_flexible_server.main.id
}

output "server_name" {
  value = azurerm_postgresql_flexible_server.main.name
}

output "fqdn" {
  value = azurerm_postgresql_flexible_server.main.fqdn
}

output "db_name" {
  value = azurerm_postgresql_flexible_server_database.app.name
}

output "db_user" {
  value = azurerm_postgresql_flexible_server.main.administrator_login
}

output "db_password" {
  value     = random_password.db.result
  sensitive = true
}

output "connection_string" {
  description = "ADO.NET-style connection string (sensitive)"
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=${var.db_name};Username=${var.db_user};Password=${random_password.db.result};SSL Mode=Require;Trust Server Certificate=false"
  sensitive   = true
}
