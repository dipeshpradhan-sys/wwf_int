using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Middleware;
using wwfpp.Models;
using wwfpp.Services;

var builder = WebApplication.CreateBuilder(args);

/** App Setting 
 * will have values like Application version | BaseUrl etc 
 */
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

/** 
 * Step 3. Email : Registration
 *  1.1 Bind SMTP Settings from appsettings.json
 *  1.2 Register your email sender service
 */
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<ISendEmails, EmailSend>();
builder.Services.AddScoped<EmailService>();

/** 
 * Register GlobalSettings as singleton
 * try to load values from pp_options
 */
builder.Services.AddSingleton<GlobalOptionServices>();

/**
 * Connection String for MS Sql server
 */
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"))
);

/** 
 * Secure Cookie Policy 
 */
builder.Services.AddCookiePolicy(options =>
{
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

/**
 * Secure Auth Cookie 
 */
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".pp.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        /* Redirects when not authenticated or not authorized*/
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/PermissionDenied";

    });
builder.Services.AddAuthorization();

/** 
 * Secure Antiforgery 
 */
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".pp.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

/**
 *   Add services required for session
 *   Needed for session storage
 */
builder.Services.AddDistributedMemoryCache();

/**
 * Secure Session
 */
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".pp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

/** if in future, required to implement no cache option,
 * uncomment the below line to use the functionality globally
 * For now only used in log out section. if below is implemented
 * make necessary changes on log out section 
 */
//builder.Services.AddControllersWithViews(options => { options.Filters.Add<NoCacheFilter>(); });

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

/** 
 * Add services to the container | MVC services.
 */
#region FOR SESSION HELPER
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionHelper>();
builder.Services.AddScoped<EmployeeServices>();
builder.Services.AddScoped<SettingsServices>();
builder.Services.AddScoped<AccountServices>();
builder.Services.AddScoped<AdminServices>();
builder.Services.AddScoped<AttendanceServices>();
builder.Services.AddScoped<LeaveServices>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<UserRightsServices>();
builder.Services.AddScoped<PayrollServices>();
builder.Services.AddScoped<PaySlipManager>();
builder.Services.AddScoped<OvertimeServices>();

#endregion

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new SubfolderViewLocationExpander());
});

/*
builder.Services.AddControllers(options =>
{
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
});
*/
var app = builder.Build();

/** 
 * Configure the static utility once
 */
GblUtilities.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

/** Global exception handling middleware 
 * Keeping this, there is no need to put try / catch blocks on end points
 * but can be kept if needed for specific error catching
 */
app.UseMiddleware<GlobalExceptionMiddleware>();

/** Load settings from DB*/
string providedPath;
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var globalOptionServices = scope.ServiceProvider.GetRequiredService<GlobalOptionServices>();

    globalOptionServices.OptionServices = dbContext.tbl_pp_options
    .Where(o => o.autoload == "Y") // optional filter
    .ToDictionary(o => o.option_name!, o => o.option_value!);

    providedPath = globalOptionServices.OptionServices["op_document_file_path_out"];
}

/**
 * Configure the HTTP request pipeline.
 * Set the Application environment 
 * Remember to set the values in launchsettings file
 * UseHsts() -> Strict-Transport-Security: max-age=31536000
 * The default HSTS value is 30 days. 
 * You may want to change this for production 
 * scenarios, see https://aka.ms/aspnetcore-hsts.
 */
if (app.Environment.IsDevelopment())
{
    var applicationBuilder = app.UseDeveloperExceptionPage();
}
else
{
    var applicationBuilder = app.UseExceptionHandler("~/Home/Error");
    var aapsehsts = app.UseHsts();
}

/**
 * Other middleware
 */

/**Redirect HTTP ? HTTPS*/
app.UseHttpsRedirection();
app.UseStaticFiles();

/**Must be before UseAuthentication*/
app.UseCookiePolicy();
app.UseRouting();
app.UseSession();

/**  Authentication 
 *  WHO are you?      
 *  -> Login, Identity, Session
 */
app.UseAuthentication();

/**
 * Security Headers
' * Prevent Click Hijacking: CWE-693
' * To effectively prevent framing attacks, the application should return a response header with the
' * name X-Frame-Options and the value DENY preventing framing altogether, or the value SAMEORIGIN
' * to allow framing only by pages on the same origin as the response itself. Note that the SAMEORIGIN
' * header can be partially bypassed if the application itself can be made to frame untrusted websites.
' * Date : 2026-Jun-26
' */
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'none'");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");

    if (context.User.Identity?.IsAuthenticated == true)
    {
        /** ...but your custom session is gone **/
        string? LoginId = context.Session.GetString("login_id");
        if (string.IsNullOrEmpty(LoginId))
        {
            /** Force sign out the stale .NET cookie */
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            context.Response.Redirect("/Account/Login");
            return;
        }
    }
    await next().ConfigureAwait(true);
});

/** Authorization  
 * WHAT can you do?  
 * -> Permissions, Roles, Access
 */
app.UseAuthorization();

app.UseStaticFiles(); // for wwwroot

if (app.Environment.IsDevelopment())
{
    // Add this for external folder
    _ = app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "web-content")),
        RequestPath = "/uploads"
    });
}
else
{
    // Map UNC path
    _ = app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(providedPath, "web-content")),
        RequestPath = "/uploads"
    });
}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Register once
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

app.Run();