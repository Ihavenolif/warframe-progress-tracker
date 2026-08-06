# Kubernetes deployment

`base` contains environment-neutral application resources only:

- Vue Router SPA served by Nginx, with an internal API reverse proxy.
- ASP.NET Core backend.
- Data updater, exposed only inside the cluster.
- imgproxy.

Ingress, TLS/ACME, PostgreSQL, namespaces, resource sizing, image pull credentials, and secret management belong to the target environment.

## Environment contract

Create these resources in the deployment namespace, normally through an overlay or external secret controller:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: warframe-tracker-environment
data:
  DB_HOST: postgres.example.internal
  POSTGRES_DB: warframe_tracker
  POSTGRES_USER: warframe_tracker
  ORIGIN_URL: tracker.example.com
---
apiVersion: v1
kind: Secret
metadata:
  name: warframe-tracker-secrets
type: Opaque
stringData:
  POSTGRES_PASSWORD: replace-me
  JWT_KEY: replace-with-a-long-random-key
```

Do not commit real secret values. `ORIGIN_URL` is hostname only, without scheme.

Configure environment-managed Ingress routes as follows:

- Application paths, including `/api/` and `/swagger/`: `warframe-tracker-frontend:80`.
- `/images/`: `warframe-tracker-imgproxy:8080`, rewriting the path as required by imgproxy.

Frontend forwards API requests to backend internally. It does not proxy or cache image requests. No application pod needs certificates.
All frontend paths use the same `index.html`; Vue Router resolves routes client-side.

## Overlay

Minimal overlay:

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

namespace: warframe-tracker

resources:
  - ../../base
  - environment-config.yaml

images:
  - name: ghcr.io/ihavenolif/warframe-progress-tracker-frontend
    newTag: sha-or-release-tag
  - name: ghcr.io/ihavenolif/warframe-progress-tracker-backend
    newTag: sha-or-release-tag
  - name: ghcr.io/ihavenolif/warframe-progress-tracker-data-update
    newTag: sha-or-release-tag
```

Render before applying:

```sh
kubectl kustomize k8s/overlays/<environment>
kubectl apply -k k8s/overlays/<environment>
```

Pin all images in production. Base uses `latest` only as a replaceable template default.
