using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using FormFillerCore.Repository.RepModels;
using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.Repositories;
using FormFillerCore.Service.Interfaces;
using FormFillerCore.Service.Services;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri($"https://{builder.Configuration.GetSection("KeyVaultData")["KeyVaultName"]}.vault.azure.net/");

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    { 
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.NameClaimType = "Application Token";
    }, options => { builder.Configuration.Bind("AzureAd", options); });

//builder.Services.AddAuthorization(config =>
//{
//    config.AddPolicy("AuthZPolicy", policyBuilder =>
//        policyBuilder.Requirements.Add(new ScopeAuthorizationRequirement() { RequiredScopesConfigurationKey = $"AzureAd:Scopes" }));
//});
//EventLog.WriteEntry(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToString());

EventLog.WriteEntry(".NET Runtime","The Environment is: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));


if (builder.Environment.IsProduction())
{
    EventLog.WriteEntry(".NET Runtime", "Attempting Production Build");
    
    using (var x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
    {
        x509Store.Open(OpenFlags.ReadOnly);
        var x509Cert = x509Store.Certificates
            .Find(
                X509FindType.FindByThumbprint,
                builder.Configuration.GetSection("KeyVaultData")["AzureADThumbprint"].ToString(),
                validOnly: false)
            .OfType<X509Certificate2>()
            .SingleOrDefault();

        builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new ClientCertificateCredential(
            builder.Configuration.GetSection("KeyVaultData")["AzureADDirectoryID"],
            builder.Configuration.GetSection("KeyVaultData")["AzureADAppID"],
            x509Cert));
    }
}
else
{
    EventLog.WriteEntry(".NET Runtime", "Attempting Development Build");

    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

var connectionString = builder.Configuration["FormFillerConn"];

connectionString = connectionString.Replace("\\\\", "\\");

builder.Services.AddDbContext<PdfformFillerContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddScoped<IDataMapItemRepository, DataMapItemRepository>();
builder.Services.AddScoped<IDataTypeRepository, DataTypeRepository>();
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddScoped<IDataMapService, DataMapService>();
builder.Services.AddScoped<IFormAPIService, FormAPIService>();
builder.Services.AddScoped<IFormsService, FormsService>();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapControllers();

app.Run();
