@echo off
REM Generate TypeScript models from the API OpenAPI schema

set API_URL=http://localhost:5201
set OUTPUT_DIR=src\models

mkdir %OUTPUT_DIR% 2>nul

echo Fetching OpenAPI schema from %API_URL%/swagger/v1/swagger.json...
call npx openapi-typescript %API_URL%/swagger/v1/swagger.json -o %OUTPUT_DIR%\api.ts

echo Models generated in %OUTPUT_DIR%\api.ts
