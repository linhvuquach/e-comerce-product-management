terraform {
  backend "azurerm" {
    resource_group_name  = "pm-tfstate-rg"
    storage_account_name = "pmtfstateprod"
    container_name       = "tfstate"
    key                  = "prod.terraform.tfstate"
    use_oidc             = true
  }
}
