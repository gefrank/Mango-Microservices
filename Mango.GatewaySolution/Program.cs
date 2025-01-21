using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthentication();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// This will automatically configure Ocelot per the json file above.
builder.Services.AddOcelot(builder.Configuration);


var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot().GetAwaiter().GetResult();  
app.Run();
