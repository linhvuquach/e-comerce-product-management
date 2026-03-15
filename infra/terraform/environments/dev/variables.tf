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
  default = ""
}

variable "jwt_audience" {
  type    = string
  default = "api://product-management"
}

variable "tags" {
  type    = map(string)
  default = {}
}
