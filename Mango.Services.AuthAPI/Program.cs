using Mango.MessageBus;
using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services Zone - BEGIN

// Define the class the implements AppDbContext
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Allows access to ApiSettings in JwtOptions class
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));

// NOTE: Using defaults for Authentication IdentityUser
// instead we are using the extended ApplicationUser 
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>() // AddEntityFrameworkStores acts a bridge between EF Core and .NET Identity 
    .AddDefaultTokenProviders(); 

builder.Services.AddControllers();
// Scoped (AddScoped): A new instance is created once per request.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseAuthentication(); // Added, must always come before UseAuthorization
app.UseAuthorization();

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