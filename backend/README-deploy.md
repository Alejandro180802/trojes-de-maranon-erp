# Backend deploy — Google Cloud Run

The API is containerized ([Dockerfile](Dockerfile)) and deploys to Cloud Run. The
gcloud project does not exist yet — create it, then fill in `PROJECT_ID` / `REGION` below.

## Prerequisites

```bash
gcloud auth login
gcloud config set project PROJECT_ID
gcloud services enable run.googleapis.com artifactregistry.googleapis.com cloudbuild.googleapis.com
```

## Deploy (build from source)

Run from the `backend/` directory (Cloud Build uses this as the Docker context):

```bash
gcloud run deploy trojes-api \
  --source . \
  --region REGION \
  --allow-unauthenticated \
  --port 8080 \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
  --set-env-vars "ConnectionStrings__DefaultConnection=Host=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.whqzeueichdpyjsefxfn;Password=SUPABASE_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;No Reset On Close=true" \
  --set-env-vars "Jwt__SecretKey=LONG_RANDOM_SECRET_AT_LEAST_32_CHARS" \
  --set-env-vars "Cors__AllowedOrigins__0=https://FIREBASE_PROJECT_ID.web.app"
```

Prefer **Secret Manager** for the connection string and JWT secret in real deployments:

```bash
echo -n "Host=...;Password=..." | gcloud secrets create trojes-db-conn --data-file=-
gcloud run deploy trojes-api --source . --region REGION \
  --set-secrets "ConnectionStrings__DefaultConnection=trojes-db-conn:latest"
```

## Notes

- Cloud Run injects `$PORT` (default 8080); the container binds Kestrel to it.
- On startup the API runs EF migrations (`Database.MigrateAsync`) and seeds the admin
  user against the configured connection string. If migrations fail through the Supabase
  **transaction pooler** (port 6543, PgBouncer), run them once against the **session**
  connection (port 5432) — either locally with `dotnet ef database update` or by pointing
  `ConnectionStrings__DefaultConnection` at the 5432 host for the first deploy.
- Add the real Firebase Hosting origin(s) to `Cors__AllowedOrigins__N` so the SPA can call the API.
