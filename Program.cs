
// Import the namespace containing application-specific data models
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
// Import Entity Framework Core for database access functionality
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

namespace EmployeeManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting application...");

                var builder = WebApplication.CreateBuilder(args);

                // --------------------- NLog Setup ---------------------
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                builder.Host.UseNLog();

                // --------------------- Services Registration ---------------------

                // Register database context first (for Identity to use it)
                builder.Services.AddDbContextPool<AppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

                // Register Identity services (IdentityUser, IdentityRole)
                builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
                { 
                    //For email verification
                    options.SignIn.RequireConfirmedEmail = true;
                    //Custom token validation for email
                    options.Tokens.EmailConfirmationTokenProvider = "CustomEmailProvider";
                    //Login attempts
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddEntityFrameworkStores<AppDBContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<IdentityUser>> ("CustomEmailProvider");                

                // Register MVC services
                builder.Services.AddControllersWithViews().AddViewOptions(options => { options.HtmlHelperOptions.ClientValidationEnabled = true;}); // ✅ Better than AddMvc() in modern ASP.NET Core

                //Initiates Policy based Autorization
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("CanCreateUserPolicy", policy => policy.RequireClaim("Create User", "Create User")); // Matches both Type & Value
                    options.AddPolicy("CanEditUserPolicy", policy => policy.RequireClaim("Edit User", "Edit User")); // Matches both Type & Value
                    options.AddPolicy("CanDeleteUserPolicy", policy => policy.RequireClaim("Delete User", "Delete User")); // Matches both Type & Value
                    options.AddPolicy("CanViewUserPolicy", policy => policy.RequireClaim("User View", "User View")); // Matches both Type & Value

                    options.AddPolicy("CanCreateRolePolicy", policy => policy.RequireClaim("Create Role", "Create Role")); // Matches both Type & Value
                    options.AddPolicy("CanEditRolePolicy", policy => policy.RequireClaim("Edit Role", "Edit Role")); // Matches both Type & Value
                    options.AddPolicy("CanDeleteRolePolicy", policy => policy.RequireClaim("Delete Role", "Delete Role")); // Matches both Type & Value
                    options.AddPolicy("CannotSelfEditRolePolicy", policy => policy.Requirements.Add(new ManageAdminRolesAndClaimRequirement()));
                });

                builder.Services.AddHttpContextAccessor();                

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie() //Needed for app to handle login cookies
                ;

                //Idle Time logout
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Auto logout after 10 mins of inactivity
                    options.SlidingExpiration = true; // Resets timer on activity
                    options.LoginPath = "/Account/Login"; // Ensure this is correct

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.Redirect("/Account/Login");
                        return Task.CompletedTask;
                    };
                });

                //Set the token lifespan
                builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(5));
                //Set the token lifespan for email (Custom Token)
                builder.Services.Configure<CustomEmailConfirmationTokenProviderOption>(options => options.TokenLifespan = TimeSpan.FromDays(1));

                // Register custom Employee repository
                builder.Services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();
                builder.Services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
                builder.Services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
                builder.Services.AddSingleton<DataProtectionPurposeStrings>();

                // --------------------- Middleware Pipeline ---------------------

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }                   

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();

                // 🔐 Enable authentication and authorization middlewares
                app.UseAuthentication();
                app.UseAuthorization();                

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");

                app.Run();
            }
            catch (Exception ex) {
                logger.Error(ex, "Stopped program because of exception");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
