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

## Cluster Config

### 1. Install Traefik with Gateway API support

```bash
helm repo add traefik https://traefik.github.io/charts
helm repo update
helm install traefik traefik/traefik \
  --namespace traefik --create-namespace \
  --set providers.kubernetesGateway.enabled=true
```

### 2. Install cert-manager CA issuers

The Aspire gateway uses a `local-ca` ClusterIssuer for TLS. Create the self-signed CA chain:

```bash
kubectl apply -f - <<'EOF'
---
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: selfsigned
spec:
  selfSigned: {}
---
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: local-ca-cert
  namespace: cert-manager
spec:
  isCA: true
  commonName: local-ca
  secretName: local-ca-secret
  issuerRef:
    name: selfsigned
    kind: ClusterIssuer
  privateKey:
    algorithm: ECDSA
    size: 256
---
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: local-ca
spec:
  ca:
    secretName: local-ca-secret
EOF
```

Verify all issuers are ready:

```bash
kubectl get clusterissuer
```

### 3. Create the wildcard TLS certificate

The gateway references a `localtest-gateway-tls` secret. Create it:

```bash
kubectl apply -f - <<'EOF'
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: localtest-gateway-tls
  namespace: default
spec:
  secretName: localtest-gateway-tls
  issuerRef:
    name: local-ca
    kind: ClusterIssuer
  dnsNames:
    - "*.localtest.me"
EOF
```

Verify:

```bash
kubectl get certificate -A
```

### 4. Deploy with Aspire

```bash
aspire deploy --environment k8s --kube-context minikube --namespace default
```

### 5. Patch the gateway ports

Aspire generates the gateway with standard ports (80/443), but Traefik's entrypoints listen on internal ports 8000/8443. After each `aspire deploy`, patch the gateway:

```bash
kubectl patch gateway localtest-gateway --type='json' -p='[
  {"op": "replace", "path": "/spec/listeners/0/port", "value": 8000},
  {"op": "replace", "path": "/spec/listeners/1/port", "value": 8443}
]'
```

Verify the gateway is accepted and programmed:

```bash
kubectl get gateway localtest-gateway
```

Both `ACCEPTED` and `PROGRAMMED` should show as `True` with `attachedRoutes > 0`.

### 6. Access the services

Start port forwarding in a separate terminal:

```bash
sudo kubectl port-forward -n traefik svc/traefik 80:80 443:443
```

The `*.localtest.me` domain resolves to `127.0.0.1` via public DNS, so no `/etc/hosts` changes are needed.

| URL | Service |
|---|---|
| `https://ui.localtest.me` | Vite frontend |
| `https://api.localtest.me` | ALB.Api backend |
| `https://auth.localtest.me` | Zitadel (identity provider) |
| `https://vault.localtest.me` | HashiCorp Vault |
| `https://mailpit.localtest.me` | Mailpit (if UseMailpit feature flag enabled) |

**Note:** The TLS certificate is self-signed. Your browser will show a certificate warning -- accept it to proceed.
