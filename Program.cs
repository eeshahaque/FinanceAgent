using System.Globalization;
using FinanceAgent.Agents;
using FinanceAgent.Components;
using FinanceAgent.Data;
using FinanceAgent.Services;
using FinanceAgent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Money is formatted as USD regardless of the host machine's locale, with "-$5.00" rather than "($5.00)"
var culture = new CultureInfo("en-US");
culture.NumberFormat.CurrencyNegativePattern = 1;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor Server keeps one DI scope per browser tab, so a single scoped DbContext would be shared
// by every click on the page. A factory gives each operation its own short-lived context.
builder.Services.AddDbContextFactory<FinanceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceDb")));

// Accounts: username + password via ASP.NET Core Identity, stored in the same SQL Server database
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
    })
    .AddEntityFrameworkStores<FinanceContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<UserDbContextFactory>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionImportService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<FinanceQueryService>();
builder.Services.AddScoped<RuleBasedCategorizer>();
builder.Services.AddScoped<AnomalyDetectionAgent>();
builder.Services.AddScoped<BudgetAdvisorAgent>();
builder.Services.AddScoped<AgentPipeline>();

var app = builder.Build();

// Creates the database / applies pending migrations. Each user's defaults are seeded when they register.
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<FinanceContext>>().CreateDbContextAsync())
{
    await DbSeeder.MigrateAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("~/account/login");
});

app.Run();
