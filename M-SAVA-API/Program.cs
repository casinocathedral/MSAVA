using M_SAVA_BLL.Services;
using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

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

    // (Optional) Support for file uploads or other filters
    // c.OperationFilter<OctetStreamOperationFilter>();
});

// Register main database context
builder.Services.AddDbContext<BaseDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabaseConnection")));

// Register services and repositories
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReturnFileService>();
builder.Services.AddScoped<SaveFileService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<FileDataSearchService>();

builder.Services.AddScoped<IIdentifiableRepository<UserDB>, IdentifiableRepository<UserDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteCodeDB>, IdentifiableRepository<InviteCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessCodeDB>, IdentifiableRepository<AccessCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessGroupDB>, IdentifiableRepository<AccessGroupDB>>();
builder.Services.AddScoped<IIdentifiableRepository<JwtDB>, IdentifiableRepository<JwtDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileDataDB>, IdentifiableRepository<SavedFileDataDB>>();
builder.Services.AddScoped<ISavedFileRepository, SavedFileRepository>();
builder.Services.AddScoped<IFileDataSearchRepository, FileDataSearchRepository>();

// JWT Authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "TemporarySuperSecretKey_ChangeMe_NOW_1234"))
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

app.UseAuthentication(); // <<== THIS IS NEEDED!
app.UseAuthorization();

app.MapControllers();

app.Run();
