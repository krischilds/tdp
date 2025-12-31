using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for local dev (Vite default port 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT auth setup (RS256-like). The RSA key will be generated at startup and stored in a singleton.
var rsa = RSA.Create(2048);
var rsaKey = new RsaSecurityKey(rsa) { KeyId = Guid.NewGuid().ToString("N") };
builder.Services.AddSingleton<RSA>(rsa);
builder.Services.AddSingleton(rsaKey);

var issuer = "http://localhost:5201";
var audience = "tdp-api";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Keep original JWT claim names (so "sub" stays as-is)
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = rsaKey
    };
});

// App-specific services
builder.Services.AddSingleton<Tdp.Api.Infrastructure.DbConnectionFactory>();
builder.Services.AddSingleton<Tdp.Api.Infrastructure.DbInitializer>();
builder.Services.AddScoped<Tdp.Api.Services.TokenService>();

var app = builder.Build();

// Initialize DB at startup
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<Tdp.Api.Infrastructure.DbInitializer>();
    await dbInit.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
