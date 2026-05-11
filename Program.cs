using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services;
using VerificarApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VerificarApi",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token JWT no campo abaixo."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("JWT Key não configurada.");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// HttpClient nomeado para a API FIPE com timeout de 15s
builder.Services.AddHttpClient<VerificarApi.Services.FipeService>(client =>
{
    client.BaseAddress = new Uri("https://parallelum.com.br/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "VerificarApi/1.0");
});
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IFipeService, VerificarApi.Services.FipeService>();

// HttpClient para o serviço de cache/sync (timeout maior pois faz muitas chamadas)
builder.Services.AddHttpClient<VerificarApi.Services.FipeCacheService>(client =>
{
    client.BaseAddress = new Uri("https://parallelum.com.br/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "VerificarApi/1.0");
});
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IFipeCacheService, VerificarApi.Services.FipeCacheService>();

// HttpClient para a API de Placas
builder.Services.AddHttpClient<VerificarApi.Services.PlacaService>(client =>
{
    client.BaseAddress = new Uri("https://wdapi2.com.br/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "VerificarApi/1.0");
});
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IPlacaService, VerificarApi.Services.PlacaService>();

// Serviços do Ciclo 2
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IPrecificacaoService, VerificarApi.Services.PrecificacaoService>();
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IVehicleHistoryService, VerificarApi.Services.VehicleHistoryService>();
builder.Services.AddHttpClient<VerificarApi.Services.VehicleAnalysisService>();
builder.Services.AddScoped<VerificarApi.Services.Interfaces.IVehicleAnalysisService, VerificarApi.Services.VehicleAnalysisService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();