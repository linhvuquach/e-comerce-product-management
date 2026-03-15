#!/usr/bin/env bash
# bootstrap-backend.sh
# Creates the Azure Storage Account and container used as the Terraform remote state backend.
# Run ONCE per environment before the first `terraform init`.
#
# Usage:
#   ./infra/scripts/bootstrap-backend.sh --env dev --location australiaeast
#   ./infra/scripts/bootstrap-backend.sh --env prod --location australiaeast

set -euo pipefail

# ---------- defaults ----------
ENV=""
LOCATION="australiaeast"
APP_PREFIX="pm"

# ---------- parse args ----------
while [[ $# -gt 0 ]]; do
  case "$1" in
    --env)      ENV="$2";      shift 2 ;;
    --location) LOCATION="$2"; shift 2 ;;
    --prefix)   APP_PREFIX="$2"; shift 2 ;;
    *) echo "Unknown argument: $1"; exit 1 ;;
  esac
done

if [[ -z "$ENV" ]]; then
  echo "Error: --env is required (dev | staging | prod)"
  exit 1
fi

# ---------- derived names ----------
RG_NAME="${APP_PREFIX}-tfstate-rg"
SA_NAME="${APP_PREFIX}tfstate${ENV}"   # storage account names: 3-24 lowercase alphanumeric
CONTAINER_NAME="tfstate"
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

echo ""
echo "=== Terraform Backend Bootstrap ==="
echo "  Environment : $ENV"
echo "  Location    : $LOCATION"
echo "  Resource Grp: $RG_NAME"
echo "  Storage Acc : $SA_NAME"
echo "  Container   : $CONTAINER_NAME"
echo "  Subscription: $SUBSCRIPTION_ID"
echo ""

# ---------- resource group ----------
echo "Creating resource group '$RG_NAME'..."
az group create \
  --name "$RG_NAME" \
  --location "$LOCATION" \
  --tags "managed-by=terraform-bootstrap" "environment=$ENV" \
  --output none

# ---------- storage account ----------
echo "Creating storage account '$SA_NAME'..."
az storage account create \
  --name "$SA_NAME" \
  --resource-group "$RG_NAME" \
  --location "$LOCATION" \
  --sku Standard_LRS \
  --kind StorageV2 \
  --allow-blob-public-access false \
  --min-tls-version TLS1_2 \
  --output none

# ---------- blob container ----------
echo "Creating container '$CONTAINER_NAME'..."
az storage container create \
  --name "$CONTAINER_NAME" \
  --account-name "$SA_NAME" \
  --auth-mode login \
  --output none

# ---------- enable versioning (protects state files) ----------
az storage account blob-service-properties update \
  --account-name "$SA_NAME" \
  --resource-group "$RG_NAME" \
  --enable-versioning true \
  --output none

echo ""
echo "=== Done. Add this backend block to environments/$ENV/backend.tf ==="
cat <<EOF

terraform {
  backend "azurerm" {
    resource_group_name  = "$RG_NAME"
    storage_account_name = "$SA_NAME"
    container_name       = "$CONTAINER_NAME"
    key                  = "$ENV.terraform.tfstate"
    use_oidc             = true
    subscription_id      = "$SUBSCRIPTION_ID"
  }
}
EOF
