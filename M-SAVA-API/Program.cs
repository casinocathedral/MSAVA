using M_SAVA_BLL.Services;
using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using M_SAVA_INF.Environment;
using Microsoft.AspNetCore.Authorization;
using M_SAVA_API.Handlers;
using M_SAVA_INF.Managers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddSingleton<IAuthorizationHandler, NotBannedHandler>();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new NotBannedRequirement())
        .Build();
});

// Swagger setup WITH JWT SUPPORT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "M-SAVA-API", Version = "v1" });

    // Add JWT Bearer Authorization to Swagger (lock button)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });

    c.OperationFilter<OctetStreamOperationFilter>();
});

// Register main database context
builder.Services.AddDbContext<BaseDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabaseConnection")));

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReturnFileService>();
builder.Services.AddScoped<SaveFileService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<SearchFileService>();

// Register repositories
builder.Services.AddScoped<IIdentifiableRepository<UserDB>, IdentifiableRepository<UserDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteCodeDB>, IdentifiableRepository<InviteCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessCodeDB>, IdentifiableRepository<AccessCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessGroupDB>, IdentifiableRepository<AccessGroupDB>>();
builder.Services.AddScoped<IIdentifiableRepository<JwtDB>, IdentifiableRepository<JwtDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileDataDB>, IdentifiableRepository<SavedFileDataDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileReferenceDB>, IdentifiableRepository<SavedFileReferenceDB>>();
builder.Services.AddScoped<IIdentifiableRepository<ErrorLogDB>, IdentifiableRepository<ErrorLogDB>>();

// Register managers
builder.Services.AddScoped<FileManager>();

// JWT Authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, 
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(M_SAVA_INF.Environment.Environment.Instance.GetSigningKeyBytes())
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline
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
