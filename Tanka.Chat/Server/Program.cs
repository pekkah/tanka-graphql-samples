using Azure.Identity;

using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;

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

services.AddHttpClient();
services.AddOptions();

/*services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    configuration.GetSection("OpenIDConnectSettings").Bind(options);

    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    options.Events = new OpenIdConnectEvents
    {
        OnTokenResponseReceived = context =>
        {
            return Task.CompletedTask;
        }
    };
});*/


services.AddControllersWithViews(options =>
     options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services.AddRazorPages().AddMvcOptions(options =>
{
    //var policy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
    //options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

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
app.MapNotFound("/api/{**segment}");
app.MapFallbackToPage("/_Host");

app.Run();
