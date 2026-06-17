using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjetApiNet.Data;
using ProjetApiNet.Repositories;
using ProjetApiNet.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de données
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
       new MySqlServerVersion(new Version(8, 0, 0))
    )
);

builder.Services.AddControllers();

// 2. Injection de dépendances : REPOSITORIES
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IGroupeTransportRepository, GroupeTransportRepository>();
builder.Services.AddScoped<ICamionRepository, CamionRepository>();
builder.Services.AddScoped<IChargementRepository, ChargementRepository>();
builder.Services.AddScoped<IZoneMiniereRepository, ZoneMiniereRepository>();

// 3. Injection de dépendances : SERVICES
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddScoped<IGroupeTransportService, GroupeTransportService>();
builder.Services.AddScoped<ICamionService, CamionService>();
builder.Services.AddScoped<IChargementService, ChargementService>();
builder.Services.AddScoped<IZoneMiniereService, ZoneMiniereService>();
builder.Services.AddScoped<IStatistiqueService, StatistiqueService>();

// pour le test des api
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. configuration de l'AUTHENTIFICATION JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "UneCleSuperSecreteDeMinimum32Caracteres!";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 5. Documentation OpenAPI / Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Appliquer les migrations automatiquement au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// IMPORTANT : L'authentification doit TOUJOURS être appelée avant l'autorisation
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();