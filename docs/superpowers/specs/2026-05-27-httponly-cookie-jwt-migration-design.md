# HttpOnly Cookie JWT Migration Design

## Overview

Migrate JWT authentication from client-side token storage (localStorage + Authorization header) to a dual-token architecture:
- **Access token** (JWT, 15-30 min): returned in response body, stored in JS memory, sent via `Authorization: Bearer` header
- **Refresh token** (Guid, 7-30 days): stored in httpOnly cookie `"refresh_token"`, sent automatically by browser

This eliminates XSS vulnerability from localStorage while maintaining stateless JWT validation.

## Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Access token storage | JS memory (not localStorage) | Not vulnerable to XSS; lost on page refresh but refreshable |
| Refresh token storage | httpOnly cookie | Not accessible to JS; sent automatically by browser |
| Refresh token format | Guid | Simple, unique, validated against DB |
| Reuse detection policy | Reject specific token only | Don't revoke all user tokens on reuse — prevents multi-device false positives |
| Backward compatibility | Clean break | No Bearer header fallback; all clients must migrate simultaneously |
| Swagger | No changes needed | Login returns access token in body; developer copies it to Authorization header |

## Section 1: Database Changes

### New Entity: `RefreshToken`

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `Guid` | Primary key |
| `Token` | `string` | Unique token (Guid) |
| `UserId` | `Guid` | FK → User.Id |
| `ExpiresAt` | `DateTime` | Expiration timestamp |
| `CreatedAt` | `DateTime` | Creation timestamp |
| `RevokedAt` | `DateTime?` | Revocation timestamp (null = active) |
| `ReplacedByToken` | `string?` | Token that replaced this one (for rotation tracking) |

### Relationships

- One `User` → many `RefreshToken` (one-to-many)
- Navigation property `ICollection<RefreshToken> RefreshTokens` on `User` entity

### Indexes

- Unique index on `Token` (for fast lookup during refresh)
- Index on `UserId` (for cleanup of old tokens)

### Rotation Logic

When a refresh token is exchanged:
1. Old token is marked: `RevokedAt = now`, `ReplacedByToken = newToken`
2. New refresh token is created and stored in DB
3. If a revoked token is reused → reject with 401, delete the cookie (do NOT revoke all user tokens)

## Section 2: Authentication Configuration (Program.cs)

### Removals

- **Delete** `AddCookie()` registration — it's incorrect for raw JWT cookies and not needed
- **Delete** `ResponseLogInApiModel` class (will be recreated with new structure)

### Changes to JwtOptions

```csharp
public class JwtOptions
{
    public string Key { get; set; }
    public int ExpiresMinutes { get; set; }      // access token lifetime (15-30 min)
    public int RefreshExpiresDays { get; set; }   // refresh token lifetime (7-30 days)
}
```

`ExpiresHours` renamed to `ExpiresMinutes`. New property `RefreshExpiresDays` added.

### JwtProvider Changes

```csharp
// Change from:
expires: DateTime.UtcNow.AddHours(options.ExpiresHours)
// To:
expires: DateTime.UtcNow.AddMinutes(options.ExpiresMinutes)
```

### AddJwtBearer — No Changes Needed

The existing `AddJwtBearer` configuration remains as-is. It reads the access token from the `Authorization: Bearer` header. No `OnMessageReceived` event is needed.

### CORS — Keep AllowCredentials()

The `AllowCredentials()` addition is correct and necessary for the browser to send cookies cross-origin. Keep it.

### Middleware Order — Keep UseCors() Before UseAuthentication()

The current ordering (`UseCors()` → `UseAuthentication()` → `UseAuthorization()`) is correct. Keep it.

### appsettings.json Updates

```json
"JwtSettings": {
    "Key": "<existing-key>",
    "ExpiresMinutes": "15",
    "RefreshExpiresDays": "7"
}
```

Remove the old `ExpiresHours` key.

## Section 3: API Endpoints

### PUT /api/user/login — Login

**Request body:** `{ email, password }` (unchanged)

**Response:** `200 OK` with body:
```json
{
    "accessToken": "<JWT>"
}
```

**Side effects:**
- Sets httpOnly cookie `"refresh_token"` with Guid token
- Cookie options: `HttpOnly=true`, `Secure` (production only), `SameSite=Lax`, `Path=/`, `Expires` from `RefreshExpiresDays`

**Service changes:** `IUserService.LogInAsync` returns `(string AccessToken, string RefreshToken)` instead of `string`.

**Note:** Refresh token is generated as a Guid (Guid.NewGuid()), not via JwtProvider.

### POST /api/user/refresh — Refresh Tokens (NEW)

**Request:** No body. Browser sends `"refresh_token"` cookie automatically.

**Response on success:** `200 OK` with body:
```json
{
    "accessToken": "<new-JWT>"
}
```

**Side effects:**
- Sets new httpOnly cookie `"refresh_token"` with new Guid token
- Old refresh token marked as revoked in DB

**Error responses:**
- `401 Unauthorized` — cookie missing, token not found, expired, or revoked
- On revoked token reuse: delete cookie, return 401

### POST /api/user/logout — Logout (NEW)

**Request:** No body. Requires `Authorization: Bearer <access_token>` header.

**Response:** `200 OK`

**Side effects:**
- Marks refresh token as revoked in DB (if cookie present)
- Deletes `"refresh_token"` cookie

### Response Models

```csharp
public class ResponseLogInApiModel
{
    public string AccessToken { get; set; }
}

public class ResponseRefreshApiModel
{
    public string AccessToken { get; set; }
}
```

### Service Layer Details

**IUserService changes:**

- `LogInAsync` → returns `(string AccessToken, string RefreshToken)` instead of `string`
  - AccessToken: generated by `IJwtProvider.GenerateToken()`
  - RefreshToken: generated as Guid (Guid.NewGuid()), stored in DB via repository

- `RefreshAsync(string refreshToken)` → returns `(string AccessToken, string RefreshToken)`
  - Validates refresh token against DB (exists, not expired, not revoked)
  - If revoked token reused → reject, delete cookie
  - Creates new access token via `IJwtProvider.GenerateToken()`
  - Creates new refresh token, marks old one as revoked

- `LogoutAsync(string refreshToken)` → `void`
  - Marks refresh token as revoked in DB

## Section 4: Swagger — No Changes

The login endpoint returns the access token in the response body. Developers can:
1. Call `/api/user/login` → copy `accessToken` from response
2. Click "Authorize" in Swagger UI → paste token as Bearer
3. All subsequent requests work via `Authorization: Bearer` header

No Swagger configuration changes needed.

## Section 5: Frontend Specification (Separate Repository)

### Remove

- JWT storage in `localStorage` / `sessionStorage`
- Axios interceptor reading token from localStorage

### Add/Change

1. **Access token in JS memory** — Store in Zustand store or React context. Lost on page refresh; recovered via `/api/user/refresh`.

2. **Axios request interceptor:**
```typescript
api.interceptors.request.use((config) => {
  const token = authStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

3. **Axios response interceptor (401 → auto-refresh):**
```typescript
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      const newToken = await refreshToken(); // POST /api/user/refresh
      authStore.getState().setAccessToken(newToken);
      error.config.headers.Authorization = `Bearer ${newToken}`;
      return api(error.config);
    }
    authStore.getState().clearAuth();
    window.location.href = '/login';
    return Promise.reject(error);
  }
);
```

4. **Configure `withCredentials: true`:**
```typescript
const api = axios.create({ baseURL: '...', withCredentials: true });
```

5. **Logout:** Call `POST /api/user/logout`, clear access token from memory.

### API Contract Summary

| Endpoint | Method | Request Body | Response Body | Cookie |
|----------|--------|-------------|---------------|--------|
| `/api/user/login` | PUT | `{ email, password }` | `{ accessToken }` | Set: `refresh_token` |
| `/api/user/refresh` | POST | — | `{ accessToken }` | Set: `refresh_token` |
| `/api/user/logout` | POST | — | — | Delete: `refresh_token` |

## Section 6: Database Migration

After creating the `RefreshToken` entity and updating `AppDbContext`, generate and apply the EF Core migration:

```bash
dotnet ef migrations add AddRefreshToken -p DAL/Context -s SmartWallet
dotnet ef database update -p DAL/Context -s SmartWallet
```

Where:
- `-p` — path to the project containing the DbContext
- `-s` — path to the project containing Program.cs (the startup project)

## Files to Modify

| File | Change |
|------|--------|
| `SmartWallet/Program.cs` | Remove `AddCookie()`, update `JwtOptions` binding, remove `ExpiresHours` config |
| `SmartWallet/Controllers/UserController.cs` | Rewrite `LogIn`, add `Refresh`, add `LogOut` endpoints |
| `Services/UserService.cs` | Update `LogInAsync` return type, add `RefreshAsync`, add `LogoutAsync`, add refresh token generation and validation logic |
| `Services.Contracts/IUserService.cs` | Update interface |
| `Services.Infrastructure/JwtProvider.cs` | Change `AddHours` to `AddMinutes` |
| `SmartWallet.Options/JwtOptions.cs` | Rename `ExpiresHours` → `ExpiresMinutes`, add `RefreshExpiresDays` |
| `DAL/Entities/RefreshToken.cs` | New entity |
| `DAL/Entities/User.cs` | Add `RefreshTokens` navigation property |
| `DAL/Context/AppDbContext.cs` | Add `DbSet<RefreshToken>`, configure entity |
| `SmartWallet/Models/Account/ResponseLogInApiModel.cs` | Update to `{ AccessToken }` |
| `SmartWallet/Models/Account/ResponseRefreshApiModel.cs` | New model |
| `SmartWallet/appsettings.json` | Update `JwtSettings` section |

## Files to Delete

| File | Reason |
|------|--------|
| None | `ResponseLogInApiModel` is updated, not deleted |