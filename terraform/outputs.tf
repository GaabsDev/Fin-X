# Terraform outputs for FinX deployment

output "resource_group_name" {
  value       = azurerm_resource_group.finx.name
  description = "Name of the resource group"
}

output "api_url" {
  value       = "http://${azurerm_container_group.finx_api.fqdn}:5006"
  description = "Public URL for FinX API"
}

output "swagger_url" {
  value       = "http://${azurerm_container_group.finx_api.fqdn}:5006/swagger"
  description = "Swagger documentation URL"
}

output "api_ip_address" {
  value       = azurerm_container_group.finx_api.ip_address
  description = "Public IP address of the API"
}

output "cosmos_db_endpoint" {
  value       = azurerm_cosmosdb_account.finx_mongo.endpoint
  description = "Cosmos DB endpoint"
}

output "cosmos_db_connection_string" {
  value       = azurerm_cosmosdb_account.finx_mongo.connection_strings[0]
  description = "Cosmos DB MongoDB connection string"
  sensitive   = true
}

output "log_analytics_workspace_id" {
  value       = azurerm_log_analytics_workspace.finx.id
  description = "Log Analytics Workspace ID for monitoring"
}

output "container_registry_login_server" {
  value       = azurerm_container_registry.finx_acr.login_server
  description = "Container Registry login server"
}

# Deployment summary
output "deployment_summary" {
  value = {
    environment           = var.environment
    location             = var.location
    api_url              = "http://${azurerm_container_group.finx_api.fqdn}:5006"
    swagger_url          = "http://${azurerm_container_group.finx_api.fqdn}:5006/swagger"
    postman_collection   = "Update base_url to: http://${azurerm_container_group.finx_api.fqdn}:5006"
    scripts_container    = azurerm_container_group.finx_scripts.name
  }
  description = "Summary of deployed resources and URLs"
}