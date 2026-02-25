using SistemaVenta.IOC;
using Scalar.AspNetCore; // <--- Importante

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 1. OpenApi Nativo (Funciona con Scalar)
builder.Services.AddOpenApi();

builder.Services.InyectarDependencias(builder.Configuration);

// --- Configuración CORS ---
builder.Services.AddCors(options => {
    options.AddPolicy("NuevaPolitica", app => {
        app.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// --- Configuración JWT ---
builder.Services.AddScoped<SistemaVenta.Utility.Seguridad.IJwtService, SistemaVenta.Utility.Seguridad.JwtService>();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "Clave_De_Respaldo_Segura_123"))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 2. Configurar Scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("NuevaPolitica");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();