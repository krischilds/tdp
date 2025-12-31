#!/usr/bin/env bash
# Generate TypeScript models from the API OpenAPI schema

API_URL="http://localhost:5201"
OUTPUT_DIR="./src/models"

mkdir -p "$OUTPUT_DIR"

echo "Fetching OpenAPI schema from $API_URL/swagger/v1/swagger.json..."
npx openapi-typescript "$API_URL/swagger/v1/swagger.json" -o "$OUTPUT_DIR/api.ts"

echo "Models generated in $OUTPUT_DIR/api.ts"
