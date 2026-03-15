# Install ArgoCD onto the AKS cluster via Terraform helm_release.
# This ensures the cluster and GitOps controller are provisioned in one pass.

resource "helm_release" "argocd" {
  name             = "argocd"
  repository       = "https://argoproj.github.io/argo-helm"
  chart            = "argo-cd"
  version          = "7.4.4"
  namespace        = "argocd"
  create_namespace = true
  wait             = true
  timeout          = 600

  values = [
    <<-EOT
    server:
      extraArgs:
        - --insecure    # TLS terminated at ingress-nginx
    configs:
      params:
        server.insecure: "true"
    EOT
  ]

  depends_on = [azurerm_kubernetes_cluster.main]
}

# ArgoCD Image Updater — watches ACR for new image tags and commits tag updates to git
resource "helm_release" "argocd_image_updater" {
  name             = "argocd-image-updater"
  repository       = "https://argoproj.github.io/argo-helm"
  chart            = "argocd-image-updater"
  version          = "0.9.6"
  namespace        = "argocd"
  create_namespace = false
  wait             = true

  depends_on = [helm_release.argocd]
}
