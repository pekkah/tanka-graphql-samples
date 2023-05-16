using System.Net;

using Azure.Identity;

using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;

using Tanka.GraphQL.Samples.Chat.Server;
using Tanka.GraphQL.Samples.Chat.Shared.Defaults;

using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var env = builder.Environment;

// Use azure configuration manager
string connectionString = configuration.GetConnectionString("AppConfig") ?? throw new InvalidOperationException("AppConfig connection string missing");
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString);
    options.ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
});

services.AddAntiforgery(options =>
{
    options.HeaderName = AntiforgeryDefaults.HeaderName;
    options.Cookie.Name = AntiforgeryDefaults.CookieName;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAD"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

services.AddControllersWithViews(options =>
     options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services.AddRazorPages().AddMvcOptions(options =>
{
    //var policy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
    //options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

services.AddHttpForwarder()
    .AddSingleton<IForwarderHttpClientFactory, ForwarderHttpClientFactory>();

var app = builder.Build();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseSecurityHeaders(
    SecurityHeadersDefinitions.GetHeaderPolicyCollection(
        env.IsDevelopment(),
        configuration["AzureAD:Instance"] + "/" + configuration["AzureAD:TenantId"]));

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseNoUnauthorizedRedirect("/api");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

var forwarder = app.Services.GetRequiredService<IHttpForwarder>();
var factory = app.Services.GetRequiredService<IForwarderHttpClientFactory>();
var httpClient = factory.CreateClient(new ForwarderHttpClientContext()
{
    NewConfig = HttpClientConfig.Empty
});

app.Map("/api/{**catch-all}", async httpContext =>
{
    var error = await forwarder.SendAsync(
        httpContext,
        "https://localhost:8000",
        httpClient,
        new ForwarderRequestConfig(),
        (context, proxyRequest) =>
        {
            var requestPath = context.Request.Path;
            PathString remaining;
            var newPath = requestPath.StartsWithSegments("/api", out remaining) ? remaining : requestPath;
            proxyRequest.RequestUri = new Uri(new Uri("https://localhost:8000"),newPath);

            return ValueTask.CompletedTask;
        });

    if (error != ForwarderError.None)
    {
        var errorFeature = httpContext.GetForwarderErrorFeature();
        var exception = errorFeature?.Exception;

        if (exception != null)
            throw exception;
    }
});

app.MapFallbackToPage("/_Host");

app.Run();
