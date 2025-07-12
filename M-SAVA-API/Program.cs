using M_SAVA_BLL.Services;
using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
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
builder.Services.AddScoped<FileDataSearchService>();

// Register repositories
builder.Services.AddScoped<IIdentifiableRepository<UserDB>, IdentifiableRepository<UserDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteCodeDB>, IdentifiableRepository<InviteCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessCodeDB>, IdentifiableRepository<AccessCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessGroupDB>, IdentifiableRepository<AccessGroupDB>>();
builder.Services.AddScoped<IIdentifiableRepository<JwtDB>, IdentifiableRepository<JwtDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileDataDB>, IdentifiableRepository<SavedFileDataDB>>();
builder.Services.AddScoped<ISavedFileRepository, SavedFileRepository>();
builder.Services.AddScoped<IFileDataSearchRepository, FileDataSearchRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
