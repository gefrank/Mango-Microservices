using AutoMapper;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Extensions;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Services;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services Zone - BEGIN
// Define the class the implements AppDbContext
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add HTTP Client services - Register UserService first
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IUserService, UserService>();
builder.Services.AddScoped<IUserService, UserService>();

// Setup RewardService - UPDATED: Get UserService from DI
var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

// Create a scope to resolve UserService
var serviceProvider = builder.Services.BuildServiceProvider();
var userService = serviceProvider.GetRequiredService<IUserService>();

// For the service bus
builder.Services.AddSingleton<RewardService>(provider =>
    new RewardService(
        optionBuilder.Options,
        provider.GetRequiredService<IMapper>()
    ));

// Keep your existing registration for controllers and other components
builder.Services.AddScoped<IRewardService, RewardService>(provider =>
    new RewardService(
        optionBuilder.Options,
        provider.GetRequiredService<IUserService>(),
        provider.GetRequiredService<IMapper>()
    ));

// Singleton because we only want one object for different requests.
builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

builder.Services.AddControllers();
// Add AutoMapper to the services collection and look for configurations automatically
builder.Services.AddAutoMapper(typeof(Program));

// Default settings to add Authorization to Swagger
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
});

builder.AddAppAuthentication();
builder.Services.AddAuthorization();
// Services Zone - END

// Middleware Zone - BEGIN
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; //Need this to run in HTTPS
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Without this app would ignore wwwroot folder
app.UseStaticFiles();
app.MapControllers();
// Middleware Zone - END

ApplyMigration();
app.Run();

void ApplyMigration()
{
    // Check for any pending migrations and automatically apply them
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (_db.Database.GetPendingMigrations().Any())
        {
            _db.Database.Migrate();
        }
    }
}