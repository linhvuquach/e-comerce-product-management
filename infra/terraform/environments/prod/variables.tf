variable "location" {
  type    = string
  default = "australiaeast"
}

variable "prefix" {
  type    = string
  default = "pm"
}

variable "jwt_authority" {
  type    = string
}

variable "jwt_audience" {
  type    = string
  default = "api://product-management"
}

variable "acr_secondary_locations" {
  type    = list(string)
  default = []
}

variable "tags" {
  type    = map(string)
  default = {}
}
