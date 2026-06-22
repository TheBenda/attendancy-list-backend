# Kubernetes Installation
To use the aspire deployment with Kubernetes, a cluster and container-registry are required.
For this example, I used minikube and the official docker-registry container (https://hub.docker.com/_/registry).

## Prerequisites
- Install kubectl
- Kubernetes cluster
- Container-registry
- Install Gateway Api: https://gateway-api.org/docs/install/ ('kubectl apply -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.5.1/standard-install.yaml' -  verfify by running 'kubectl get crds | grep gateway' && 'kubectl apply -f https://raw.githubusercontent.com/traefik/traefik/v3.7/docs/content/reference/dynamic-configuration/kubernetes-gateway-rbac.yml)
- Install Cert Manager: https://cert-manager.io/docs/installation/helm/ ('kubectl apply -f https://github.com/cert-manager/cert-manager/releases/latest/download/cert-manager.yaml')
- Install Traefik Gateway Fabric: https://github.com/traefik/traefik-helm-chart ('helm install traefik traefik/traefik --namespace traefik --create-namespace')

## Deployment
- run aspire deploy --environment k8s --kube-context minikube --namespace default