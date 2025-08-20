using DemoTasklist.Helpers;
using DemoTasklist.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

// Add CamundaService with API URL from configuration
builder.Services.AddScoped<CamundaService>(serviceProvider =>
{
    var camundaApiUrl = builder.Configuration.GetValue<string>("Camunda:ApiUrl");
    return new CamundaService(camundaApiUrl);
});





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

app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaskList}/{action=Index}");

app.MapControllerRoute(
    name: "tasklist",
    pattern: "{controller=TaskList}/{action=Index}");

app.MapControllerRoute(
    name: "demo",
    pattern: "{controller=DemoRequest}/{action=DemoRequest}");

app.MapControllers();

app.Run();
