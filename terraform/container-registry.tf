# Azure Container Registry for FinX Docker images

resource "azurerm_container_registry" "finx_acr" {
  name                = "${var.project_name}${var.environment}acr${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.finx.name
  location            = azurerm_resource_group.finx.location
  sku                 = "Basic"
  admin_enabled       = true

  # Network access (set to false and configure private endpoints for production)
  public_network_access_enabled = true

  tags = var.common_tags
}

# Output ACR login server for use in container instances
output "acr_login_server" {
  value       = azurerm_container_registry.finx_acr.login_server
  description = "Login server URL for Azure Container Registry"
}

output "acr_admin_username" {
  value       = azurerm_container_registry.finx_acr.admin_username
  description = "Admin username for Azure Container Registry"
  sensitive   = true
}

output "acr_admin_password" {
  value       = azurerm_container_registry.finx_acr.admin_password
  description = "Admin password for Azure Container Registry"
  sensitive   = true
}