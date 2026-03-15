#!/usr/bin/env bash
# deploy.sh
# Thin wrapper: terraform init → plan → apply, then passes outputs to helm upgrade.
#
# Usage:
#   ./infra/scripts/deploy.sh --env dev
#   ./infra/scripts/deploy.sh --env prod --image-tag abc1234

set -euo pipefail

ENV=""
IMAGE_TAG="latest"
HELM_RELEASE="product-management"
HELM_NAMESPACE="product-management"
SKIP_TERRAFORM=false
SKIP_HELM=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --env)              ENV="$2";          shift 2 ;;
    --image-tag)        IMAGE_TAG="$2";    shift 2 ;;
    --skip-terraform)   SKIP_TERRAFORM=true; shift ;;
    --skip-helm)        SKIP_HELM=true;    shift ;;
    *) echo "Unknown argument: $1"; exit 1 ;;
  esac
done

if [[ -z "$ENV" ]]; then
  echo "Error: --env is required (dev | staging | prod)"
  exit 1
fi

TF_DIR="infra/terraform/environments/$ENV"
HELM_VALUES="infra/helm/umbrella/values/$ENV.yaml"

# ---------- Terraform ----------
if [[ "$SKIP_TERRAFORM" == "false" ]]; then
  echo "=== Terraform: $ENV ==="
  terraform -chdir="$TF_DIR" init -input=false
  terraform -chdir="$TF_DIR" plan -out=tfplan -input=false
  terraform -chdir="$TF_DIR" apply -input=false tfplan
fi

# ---------- Extract outputs ----------
echo "=== Reading Terraform outputs ==="
ACR=$(terraform -chdir="$TF_DIR" output -raw acr_login_server)
DB_HOST=$(terraform -chdir="$TF_DIR" output -raw db_host)
REDIS_HOST=$(terraform -chdir="$TF_DIR" output -raw redis_host)
AKS_CLUSTER=$(terraform -chdir="$TF_DIR" output -raw aks_cluster_name)
AKS_RG=$(terraform -chdir="$TF_DIR" output -raw resource_group_name)

# ---------- AKS credentials ----------
echo "=== Getting AKS credentials ==="
az aks get-credentials \
  --resource-group "$AKS_RG" \
  --name "$AKS_CLUSTER" \
  --overwrite-existing

# ---------- Helm ----------
if [[ "$SKIP_HELM" == "false" ]]; then
  echo "=== Helm upgrade: $ENV ==="
  helm dependency update infra/helm/umbrella

  helm upgrade --install "$HELM_RELEASE" infra/helm/umbrella \
    --namespace "$HELM_NAMESPACE" \
    --create-namespace \
    --values "$HELM_VALUES" \
    --set api.image.repository="$ACR/api" \
    --set api.image.tag="$IMAGE_TAG" \
    --set frontend.image.repository="$ACR/frontend" \
    --set frontend.image.tag="$IMAGE_TAG" \
    --set api.db.host="$DB_HOST" \
    --set api.redis.host="$REDIS_HOST" \
    --wait \
    --timeout 10m
fi

echo ""
echo "=== Deploy complete: $ENV @ $IMAGE_TAG ==="
