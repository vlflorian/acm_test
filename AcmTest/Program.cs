using AcmTest;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: false);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.RegisterOptions<AcmSettings>(builder.Configuration);
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.TryAddSingleton<HttpClient>();
builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// e.g. https://localhost:4200/callback --> callback 
var redirectUri = app.Configuration.GetValue<string>("AcmSettings:FrontendRedirectUri");
var callbackPath = redirectUri.Split("/").Last(); 
app.MapControllerRoute(
    name: "callbackRoute",
    pattern: callbackPath,
    defaults: new { controller = "Callback", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();