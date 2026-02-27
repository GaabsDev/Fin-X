# Variables for FinX Azure deployment

variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "finx"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "Brazil South"
}

variable "mongodb_username" {
  description = "MongoDB username"
  type        = string
  default     = "finxadmin"
  sensitive   = true
}

variable "mongodb_password" {
  description = "MongoDB password"
  type        = string
  sensitive   = true
  validation {
    condition     = length(var.mongodb_password) >= 8
    error_message = "MongoDB password must be at least 8 characters."
  }
}

variable "container_cpu" {
  description = "CPU allocation for containers"
  type        = number
  default     = 1
}

variable "container_memory" {
  description = "Memory allocation for containers (GB)"
  type        = number
  default     = 2
}

variable "common_tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default = {
    Project     = "FinX"
    Environment = "dev"
    Owner       = "DevTeam"
    Purpose     = "Healthcare API"
  }
}

variable "allowed_ips" {
  description = "List of allowed IP addresses for firewall rules"
  type        = list(string)
  default     = ["0.0.0.0"]  # Change this to specific IPs in production
}