# Cosmos DB with MongoDB API for FinX

resource "azurerm_cosmosdb_account" "finx_mongo" {
  name                = "${var.project_name}-${var.environment}-cosmos-${random_string.suffix.result}"
  location            = azurerm_resource_group.finx.location
  resource_group_name = azurerm_resource_group.finx.name
  offer_type          = "Standard"
  kind                = "MongoDB"

  # Enable automatic failover
  enable_automatic_failover = true
  enable_multiple_write_locations = false

  # MongoDB capabilities
  capabilities {
    name = "EnableAggregationPipeline"
  }

  capabilities {
    name = "mongoEnableDocLevelTTL"
  }

  capabilities {
    name = "MongoDBv3.4"
  }

  capabilities {
    name = "EnableMongo"
  }

  # Consistency policy
  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 300
    max_staleness_prefix    = 100000
  }

  # Geographic locations
  geo_location {
    location          = azurerm_resource_group.finx.location
    failover_priority = 0
  }

  # Backup policy
  backup {
    type                = "Periodic"
    interval_in_minutes = 240
    retention_in_hours  = 720
    storage_redundancy  = "Local"
  }

  # Network rules
  ip_range_filter = join(",", var.allowed_ips)
  
  # Enable public network access (set to false for private endpoints)
  public_network_access_enabled = true

  tags = var.common_tags
}

# MongoDB database
resource "azurerm_cosmosdb_mongo_database" "finx_db" {
  name                = "finxdb"
  resource_group_name = azurerm_resource_group.finx.name
  account_name        = azurerm_cosmosdb_account.finx_mongo.name

  # Throughput configuration
  throughput = 400  # Minimum for shared throughput
}

# MongoDB collections
resource "azurerm_cosmosdb_mongo_collection" "patients" {
  name                = "patients"
  resource_group_name = azurerm_resource_group.finx.name
  account_name        = azurerm_cosmosdb_account.finx_mongo.name
  database_name       = azurerm_cosmosdb_mongo_database.finx_db.name

  default_ttl_seconds = "777"
  shard_key          = "Id"

  index {
    keys   = ["_id"]
    unique = true
  }

  index {
    keys   = ["Cpf"]
    unique = false
  }
}

resource "azurerm_cosmosdb_mongo_collection" "medicalrecords" {
  name                = "medicalrecords"
  resource_group_name = azurerm_resource_group.finx.name
  account_name        = azurerm_cosmosdb_account.finx_mongo.name
  database_name       = azurerm_cosmosdb_mongo_database.finx_db.name

  default_ttl_seconds = "777"
  shard_key          = "PatientId"

  index {
    keys   = ["_id"]
    unique = true
  }

  index {
    keys   = ["PatientId"]
    unique = false
  }
}