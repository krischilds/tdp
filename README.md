# TDP: .NET REST API + Vue 3 Web Application

A full-stack webapp with an ASP.NET Core REST API backend and Vue 3 frontend, featuring JWT authentication (Auth0-style RS256), SQLite persistence, and Element Plus UI.

## 📁 Project Structure

```
Tdp.sln
├── src/
│   ├── Tdp.Api/                   # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs        # Register, Login, Refresh, Logout
│   │   │   ├── FeaturesController.cs    # CRUD for features
│   │   │   ├── UserFeaturesController.cs # User-feature relationships
│   │   │   └── JwksController.cs        # JWKS endpoint (OIDC-like)
│   │   ├── Services/
│   │   │   ├── AuthService.cs           # Static password hashing (Argon2id)
│   │   │   └── TokenService.cs          # JWT token creation (RS256)
│   │   ├── Domain/
│   │   │   ├── User.cs
│   │   │   ├── Feature.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Infrastructure/
│   │   │   ├── DbConnectionFactory.cs   # SQLite connection pool
│   │   │   └── DbInitializer.cs         # Schema creation & seeding
│   │   ├── Models/
│   │   │   └── ResponseDto.cs           # Generic response wrapper
│   │   ├── Program.cs                    # DI setup, middleware config
│   │   └── Tdp.Api.csproj
│   │
│   └── Tdp.Web/                   # Vue 3 + Vite Frontend
│       ├── src/
│       │   ├── views/
│       │   │   ├── LoginView.vue         # Login page
│       │   │   ├── RegisterView.vue      # Registration page
│       │   │   ├── DashboardView.vue     # Home/dashboard
│       │   │   └── UserFeaturesView.vue  # Feature listing
│       │   ├── stores/
│       │   │   └── auth.ts              # Pinia auth store (token, user)
│       │   ├── router/
│       │   │   └── index.ts             # Vue Router with route guards
│       │   ├── api/
│       │   │   └── client.ts            # Axios instance w/ interceptors
│       │   ├── models/
│       │   │   └── api.ts               # Generated TypeScript types
│       │   ├── main.ts
│       │   ├── App.vue
│       │   └── style.css
│       ├── .env
│       ├── package.json
│       ├── vite.config.ts
│       ├── generate-models.sh / .bat     # Script to regenerate types
│       └── tsconfig.json
```

## 🚀 Running the Application

### Prerequisites
- **.NET 9.0 SDK** (for backend)
- **Node.js 18+** (for frontend)
- **SQLite** (bundled; created at `src/Tdp.Api/App_Data/tdp.db`)

### Backend Setup & Run

1. **Navigate to the API project:**
   ```bash
   cd src/Tdp.Api
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```
   - API starts at `http://localhost:5201`
   - SQLite DB auto-created at `App_Data/tdp.db`
   - Swagger UI available at `http://localhost:5201/swagger`
   - JWKS endpoint: `http://localhost:5201/.well-known/jwks.json`

3. **Seeded Data:**
   - **Admin user:** `admin@tdp.local` / `Admin123!`
   - **Features:** `beta_access`, `dark_mode`, `analytics`, `pro_reports`, `admin_console`

### Frontend Setup & Run

1. **Navigate to the web project:**
   ```bash
   cd src/Tdp.Web
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the Vite dev server:**
   ```bash
   npm run dev
   ```
   - Frontend available at `http://localhost:5173`

### Testing the Full Flow

1. **Open browser:** http://localhost:5173
2. **Register** (optional):
   - Fill email, password, and optional display name
   - Click "Register" → redirects to login
3. **Login** with test credentials:
   - Email: `admin@tdp.local`
   - Password: `Admin123!`
   - Click "Login" → redirects to dashboard
4. **Dashboard:**
   - Welcome message with your email
   - "Manage Features" button → view your assigned features
5. **My Features:** 
   - Lists features assigned to your user
   - Click "Back" to return to dashboard
6. **Logout:**
   - Dashboard header has "Logout" button
   - Revokes refresh token and clears tokens
   - Redirects to login

## 🔐 Authentication & Security

### JWT (RS256 + JWKS)
- **Access Token:** Valid for 15 minutes; includes `sub`, `email`, `name` claims
- **Refresh Token:** Valid for 14 days; opaque string, hashed server-side
- **Token Rotation:** Every refresh request issues new access + refresh tokens
- **JWKS Endpoint:** `/.well-known/jwks.json` exposes RSA public key (Auth0-like)

### Password Hashing
- **Algorithm:** Argon2id (salt + 64MB memory, 4 iterations)
- **Storage:** `Users.PasswordHash` in SQLite

### CORS
- **Allowed Origins:** `http://localhost:5173`, `http://127.0.0.1:5173`
- **Methods:** GET, POST, PUT, DELETE
- **Headers:** Authorization, Content-Type

## 📡 API Endpoints

### Auth
| Method | Path | Request | Response |
|--------|------|---------|----------|
| POST | `/auth/register` | `{ email, password, displayName? }` | `ResponseDto<{ userId }>` |
| POST | `/auth/login` | `{ email, password, deviceInfo? }` | `ResponseDto<{ accessToken, refreshToken, expiresIn }>` |
| POST | `/auth/refresh` | `{ refreshToken, deviceInfo? }` | `ResponseDto<{ accessToken, refreshToken, expiresIn }>` |
| POST | `/auth/logout` | `{ refreshToken }` | `ResponseDto<null>` |

### Features
| Method | Path | Auth | Response |
|--------|------|------|----------|
| GET | `/features` | ✓ | `ResponseDto<Feature[]>` |
| GET | `/features/{id}` | ✓ | `ResponseDto<Feature>` |
| POST | `/features` | ✓ | `ResponseDto<Feature>` |
| PUT | `/features/{id}` | ✓ | `ResponseDto<Feature>` |
| DELETE | `/features/{id}` | ✓ | `ResponseDto<null>` |

### User Features
| Method | Path | Auth | Response |
|--------|------|------|----------|
| GET | `/users/me/features` | ✓ | `ResponseDto<Feature[]>` |
| GET | `/users/{userId}/features` | ✓ | `ResponseDto<Feature[]>` |
| POST | `/users/{userId}/features/{featureId}` | ✓ | `ResponseDto<null>` |
| DELETE | `/users/{userId}/features/{featureId}` | ✓ | `ResponseDto<null>` |

### Public
| Method | Path | Response |
|--------|------|----------|
| GET | `/.well-known/jwks.json` | JWKS (RSA public key) |

## 📦 Response Format

### Success (2xx)
```json
{
  "success": true,
  "status": 200,
  "data": {...},
  "message": null,
  "traceId": "0HMVV...",
  "meta": null
}
```

### Error (4xx/5xx)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "...",
  "traceId": "0HMVV..."
}
```
(RFC 7807 ProblemDetails format)

## 🛠️ Development

### Generate TypeScript Models from OpenAPI
The API exposes Swagger/OpenAPI schema at `/swagger/v1/swagger.json`. To regenerate client types after API changes:

#### Windows:
```bash
cd src/Tdp.Web
.\generate-models.bat
```

#### macOS/Linux:
```bash
cd src/Tdp.Web
bash generate-models.sh
```

Or manually:
```bash
npx openapi-typescript "http://localhost:5201/swagger/v1/swagger.json" -o "src/models/api.ts"
```

Generated types will be in `src/models/api.ts` and can be imported:
```typescript
import type { Feature, ResponseDtoFeature } from '@/models/api'
```

### Database

**SQLite file location:** `src/Tdp.Api/App_Data/tdp.db`

**Tables:**
- `Users` — user login info + metadata
- `Features` — available features
- `UserFeatures` — user-feature assignments
- `RefreshTokens` — token rotation history & revocation

**Pragmas:**
- `journal_mode=WAL` — write-ahead logging (better concurrency)
- `foreign_keys=ON` — enforce FK constraints
- `busy_timeout=5000` — 5s timeout for locked DB

### Environment Variables

#### Backend (`Tdp.Api`)
Set via `appsettings.json` or `dotnet user-secrets`:
- `JWT__Issuer` (default: `http://localhost:5201`)
- `JWT__Audience` (default: `tdp-api`)
- `JWT__AccessTokenMinutes` (default: 15)
- `JWT__RefreshTokenDays` (default: 14)
- `Db__ConnectionString` (auto-set; SQLite at `App_Data/tdp.db`)
- `CORS__AllowedOrigins` (default: localhost:5173)

#### Frontend (`Tdp.Web`)
Set in `.env`:
- `VITE_API_BASE_URL` (default: `http://localhost:5201`)

### Token Storage (Frontend)

- **Access Token:** Stored in Pinia store (memory) + `localStorage` for persistence
- **Refresh Token:** Stored in `localStorage`
- **Auto-Refresh:** Axios interceptor attempts refresh on 401 (one retry)
- **Logout:** Clears tokens and revokes refresh token server-side

### Route Guards

Public routes:
- `/login`
- `/register`

Protected routes (require `Authorization` header + valid JWT):
- `/dashboard`
- `/features`

Route guard at `router/beforeEach` checks auth and redirects to login if expired.

## 🔄 Token Refresh Flow

1. **Login:** User receives `accessToken` (15 min) + `refreshToken` (14 days)
2. **API Call:** Axios injects `Authorization: Bearer {accessToken}`
3. **Token Expires:** API returns 401
4. **Interceptor Catches 401:**
   - Calls `POST /auth/refresh` with `refreshToken`
   - Server validates refresh token, issues new pair
   - Retries original request with new access token
5. **Refresh Fails:** 
   - Server returns 401 (invalid/revoked refresh token)
   - Frontend logs user out and redirects to login

## 📝 License

Internal project. All rights reserved.

## 🤝 Support

For issues, please refer to the project plan or create a GitHub issue.
