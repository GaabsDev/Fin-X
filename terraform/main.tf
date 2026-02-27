# Terraform configuration for FinX deployment on Azure
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.1"
    }
  }
}

# Configure Azure Provider
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Generate random suffix for unique naming
resource "random_string" "suffix" {
  length  = 8
  special = false
  upper   = false
}

# Resource Group
resource "azurerm_resource_group" "finx" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.location

  tags = var.common_tags
}

# Log Analytics Workspace for monitoring
resource "azurerm_log_analytics_workspace" "finx" {
  name                = "${var.project_name}-${var.environment}-law-${random_string.suffix.result}"
  location            = azurerm_resource_group.finx.location
  resource_group_name = azurerm_resource_group.finx.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = var.common_tags
}