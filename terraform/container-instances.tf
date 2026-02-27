# Azure Container Instances for FinX API

# Container Group for FinX API
resource "azurerm_container_group" "finx_api" {
  name                = "${var.project_name}-${var.environment}-api-cg"
  location            = azurerm_resource_group.finx.location
  resource_group_name = azurerm_resource_group.finx.name
  ip_address_type     = "Public"
  dns_name_label      = "${var.project_name}-${var.environment}-api-${random_string.suffix.result}"
  os_type             = "Linux"

  # Image registry credentials
  image_registry_credential {
    server   = azurerm_container_registry.finx_acr.login_server
    username = azurerm_container_registry.finx_acr.admin_username
    password = azurerm_container_registry.finx_acr.admin_password
  }

  # FinX API Container
  container {
    name   = "finx-api"
    image  = "${azurerm_container_registry.finx_acr.login_server}/finx-api:latest"
    cpu    = var.container_cpu
    memory = var.container_memory

    ports {
      port     = 5006
      protocol = "TCP"
    }

    # Environment variables
    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      ASPNETCORE_URLS       = "http://+:5006"
    }

    # Secure environment variables
    secure_environment_variables = {
      "Mongo__ConnectionString" = azurerm_cosmosdb_account.finx_mongo.connection_strings[0]
      "Mongo__Database"         = azurerm_cosmosdb_mongo_database.finx_db.name
    }

    # Liveness probe
    liveness_probe {
      http_get {
        path   = "/api/auth/login"
        port   = 5006
        scheme = "Http"
      }
      initial_delay_seconds = 30
      period_seconds        = 30
      failure_threshold     = 3
      success_threshold     = 1
      timeout_seconds       = 10
    }

    # Readiness probe  
    readiness_probe {
      http_get {
        path   = "/swagger"
        port   = 5006
        scheme = "Http"
      }
      initial_delay_seconds = 15
      period_seconds        = 20
      failure_threshold     = 3
      success_threshold     = 1
      timeout_seconds       = 5
    }
  }

  # Diagnostics
  diagnostics {
    log_analytics {
      workspace_id  = azurerm_log_analytics_workspace.finx.workspace_id
      workspace_key = azurerm_log_analytics_workspace.finx.primary_shared_key
    }
  }

  tags = var.common_tags

  depends_on = [
    azurerm_cosmosdb_account.finx_mongo,
    azurerm_container_registry.finx_acr
  ]
}

# Container Group for FinX Scripts (On-demand execution)
resource "azurerm_container_group" "finx_scripts" {
  name                = "${var.project_name}-${var.environment}-scripts-cg"
  location            = azurerm_resource_group.finx.location
  resource_group_name = azurerm_resource_group.finx.name
  ip_address_type     = "None"  # No public IP needed for scripts
  os_type             = "Linux"
  restart_policy      = "Never"  # Run once and stop

  # Image registry credentials
  image_registry_credential {
    server   = azurerm_container_registry.finx_acr.login_server
    username = azurerm_container_registry.finx_acr.admin_username
    password = azurerm_container_registry.finx_acr.admin_password
  }

  # FinX Scripts Container
  container {
    name   = "finx-scripts"
    image  = "${azurerm_container_registry.finx_acr.login_server}/finx-scripts:latest"
    cpu    = 0.5
    memory = 1

    # Environment variables
    environment_variables = {
      DOTNET_ENVIRONMENT = "Production"
    }

    # Secure environment variables
    secure_environment_variables = {
      "ConnectionStrings__MongoDB" = azurerm_cosmosdb_account.finx_mongo.connection_strings[0]
      "MongoDB__Database"          = azurerm_cosmosdb_mongo_database.finx_db.name
    }

    # Override command to run specific script
    commands = ["dotnet", "scripts.dll", "help"]
  }

  # Diagnostics
  diagnostics {
    log_analytics {
      workspace_id  = azurerm_log_analytics_workspace.finx.workspace_id
      workspace_key = azurerm_log_analytics_workspace.finx.primary_shared_key
    }
  }

  tags = merge(var.common_tags, {
    Purpose = "DESAFIO-3-Scripts"
  })

  depends_on = [
    azurerm_cosmosdb_account.finx_mongo,
    azurerm_container_registry.finx_acr
  ]
}