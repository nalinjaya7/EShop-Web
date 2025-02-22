using EShopModels;
using EShopModels.Services;
using EShopWeb.Common;
using EShopWeb.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("EShopClient", (provider, option) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    option.BaseAddress = new Uri(configuration.GetValue<string>("httpconfigs:BaseUrl"));
});
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(60); });
builder.Services.AddDataProtection().SetApplicationName("EShop");
builder.Services.AddScoped(typeof(CryptoParamsProtector));
builder.Services.AddMvc(options =>
{
    // options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
   // options.RespectBrowserAcceptHeader = true;
    options.Filters.Add(typeof(CustomActionFilter)); 
    options.ValueProviderFactories.Add(new CryptoValueProviderFactory());
}).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

var SecretKey = Encoding.ASCII.GetBytes("Jagath98989765123");
builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;   
})
.AddJwtBearer(token =>
{
    token.RequireHttpsMetadata = false;
    token.SaveToken = true;
    token.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        //Same Secret key will be used while creating the token
        IssuerSigningKey = new SymmetricSecurityKey(SecretKey),
        ValidateIssuer = true,
        //Usually, this is your application base URL
        ValidIssuer = "http://localhost:5167/",
        ValidateAudience = true,
        //Here, we are creating and using JWT within the same application.
        //In this case, base URL is fine.
        //If the JWT is created using a web service, then this would be the consumer URL.
        ValidAudience = "http://localhost:5167/",
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    token.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        { 
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; 
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnChallenge = ctx =>
        { 
            return Task.FromResult(0); 
        },
        OnMessageReceived = ctx =>
        { 
            return Task.CompletedTask;
        } 
    };    
});

builder.Services.AddControllersWithViews().AddJsonOptions(v => {
    v.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Services.Configure<EShopSystemConfig>(builder.Configuration.GetSection("httpconfigs"));
builder.Services.AddHttpContextAccessor();
//builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddJsonOptions(y => y.JsonSerializerOptions.ReferenceHandler = System.Text.Json.JsonSerializerOptions.Default.ReferenceHandler);

builder.Services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor, Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Mvc.Routing.IUrlHelperFactory, Microsoft.AspNetCore.Mvc.Routing.UrlHelperFactory>();

builder.Services.Configure<CustomFileLoggerOptions>(builder.Configuration.GetSection("Logging:CustomLoggingFile:Options"));
builder.Services.AddSingleton<ILoggerProvider, CustomFileLoggerProvider>();
//builder.Services.AddSingleton<IDeveloperPageExceptionFilter,CustomDeveloperPageExceptionFilter>();
builder.Services.Configure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/LoginUser";
    options.AccessDeniedPath = "/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{ 
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();



app.Use(async (context, next) =>
{
    string JWToken = context.Session.GetString("JWToken");
    if (!string.IsNullOrEmpty(JWToken))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
