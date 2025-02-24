using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Services Zone - BEGIN

// Add services to the container.
builder.Services.AddControllersWithViews();

// In order to inject IHttpClientFactory httpClientFactory in the BaseService.cs.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // This is the main thing.

builder.Services.AddHttpClient<ICouponService, CouponService>();
builder.Services.AddHttpClient<IProductService, ProductService>();
builder.Services.AddHttpClient<ICartService, CartService>();
builder.Services.AddHttpClient<IOrderService, OrderService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();

SD.CouponAPIBase = builder.Configuration["ServiceUrls:CouponAPI"]!;
SD.AuthAPIBase = builder.Configuration["ServiceUrls:AuthAPI"]!;
SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"]!;
SD.ShoppingCartAPIBase = builder.Configuration["ServiceUrls:ShoppingCartAPI"]!;
SD.OrderAPIBase = builder.Configuration["ServiceUrls:OrderAPI"]!;

// Register services for use.
// AddScoped: A new instance of the service is created once per request.
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessedDenied";
    });

// Services Zone - END

// Middleware Zone - BEGIN

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Middleware Zone - END

app.Run();
