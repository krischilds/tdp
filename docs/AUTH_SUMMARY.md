### Login workflow:

```
Vue Client                          .NET Server
    |                                    |
1. User fills login form (email/pwd)     |
    |                                    |
2. POST /api/auth/login                  |
    | { email, password } -------------->|
    |                                    | Validate credentials against DB
    |                                    | If valid → Create claims:
    |                                    |   - UserId
    |                                    |   - Role/Permissions
    |                                    |   - Exp (e.g., 15-60 min)
    |                                    | Generate JWT using secret key
    |                                    |
    | <----------------- 200 OK ---------|
    | { "token": "eyJhbGciOi..." }       |
    |                                    |
3. Store token (Pinia/memory)            |
4. Redirect to protected dashboard       |
```

