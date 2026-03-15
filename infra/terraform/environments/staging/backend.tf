terraform {
  backend "azurerm" {
    resource_group_name  = "pm-tfstate-rg"
    storage_account_name = "pmtfstatestaging"
    container_name       = "tfstate"
    key                  = "staging.terraform.tfstate"
    use_oidc             = true
  }
}
