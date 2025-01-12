using Mango.Services.CouponAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services Zone - BEGIN

// Define the class the implements AppDbContext
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

// Middleware Zone - END

app.Run();
