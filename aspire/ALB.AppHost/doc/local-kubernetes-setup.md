# Local Kubernetes Setup

To use the Aspire deployment with Kubernetes, a cluster and container registry are required.
This guide uses minikube and the official [Docker Registry](https://hub.docker.com/_/registry).

## Prerequisites

- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Helm](https://helm.sh/docs/intro/install/)
- A running Kubernetes cluster (e.g., minikube)
- A container registry accessible from the cluster (e.g., `localhost:5000`)
- Docker running locally

## Cluster Config

Run these steps once to prepare the cluster before the first Aspire deploy.

### 1. Install Gateway API CRDs

```bash
kubectl apply -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.5.1/standard-install.yaml
```

Verify:

```bash
kubectl get crds | grep gateway
```

### 2. Install Traefik with Gateway API support

```bash
helm repo add traefik https://traefik.github.io/charts
helm repo update
helm install traefik traefik/traefik \
  --namespace traefik --create-namespace \
  --set providers.kubernetesGateway.enabled=true
```

### 3. Install cert-manager

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.17.2/cert-manager.yaml
kubectl wait --for=condition=Available deployment --all -n cert-manager --timeout=120s
```

### 4. Create the self-signed CA chain

The Aspire gateway uses a `local-ca` ClusterIssuer for TLS:

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

### 5. Create the wildcard TLS certificate

The gateway references a `localtest-gateway-tls` secret:

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

## Deployment

### First deploy (Zitadel init)

On the first deploy, Zitadel needs to initialize its database and generate a PAT token for the Login V2 container. Set a placeholder for the PAT:

```bash
dotnet user-secrets set "Parameters:zitadel-login-pat" "placeholder" \
  --project aspire/ALB.AppHost/ALB.AppHost.csproj
```

Deploy:

```bash
aspire deploy --environment k8s --kube-context minikube --namespace default
```

The `zitadel-login` container will fail (invalid PAT). This is expected.

### Extract the Zitadel Login PAT

Log into the Zitadel console at `https://auth.localtest.me/ui/console` with the admin credentials configured during init. Navigate to the `login-client` service user and create a Personal Access Token. Then store it:

```bash
dotnet user-secrets set "Parameters:zitadel-login-pat" "<paste-the-pat-here>" \
  --project aspire/ALB.AppHost/ALB.AppHost.csproj
```

### Redeploy with the real PAT

```bash
aspire deploy --environment k8s --kube-context minikube --namespace default
```

### Subsequent deploys

After the PAT is stored in user secrets, only a single deploy is needed:

```bash
aspire deploy --environment k8s --kube-context minikube --namespace default
```

## Post-deploy steps

These steps are required after every `aspire deploy`.

### Patch the gateway ports

Aspire generates the gateway with standard ports (80/443), but Traefik's entrypoints listen on internal ports 8000/8443:

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

## Accessing the services

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
