export $(grep -v '^#' .env | xargs)

docker login ghcr.io -u "$REGISTRY_USERNAME" -p "$REGISTRY_PASSWORD"
docker compose up -d