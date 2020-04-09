using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using ProjectManagementWebApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using ProjectManagementWebApp.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;

namespace ProjectManagementWebApp
{
    public class Startup
    {
        private string _defaultCulture = "en-US";

        private CultureInfo[] _supportedCultures = new[] {
            new CultureInfo("en-US"),
            new CultureInfo("vi-VN"),
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password = new PasswordOptions
                {
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false,
                    RequiredLength = 1
                };

                options.Lockout = new LockoutOptions
                {
                    AllowedForNewUsers = true,
                    MaxFailedAccessAttempts = 5,
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5)
                };

                options.SignIn.RequireConfirmedAccount = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(5);
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: _defaultCulture, uiCulture: _defaultCulture);
                options.SupportedCultures = _supportedCultures;
                options.SupportedUICultures = _supportedCultures;
            });
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(); services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: _defaultCulture, uiCulture: _defaultCulture);
                options.SupportedCultures = _supportedCultures;
                options.SupportedUICultures = _supportedCultures;
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            CreateRoles(service).Wait();
        }

        private async Task CreateRoles(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            var roleNames = new[] { "Administrator", "Lecturer", "Student" };
            foreach (var roleName in roleNames)
            {
                var isExist = await roleManager.RoleExistsAsync(roleName);
                if (!isExist)
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                }
            }

            var dob = new DateTime(1998, 8, 18);
            var adminUser = new ApplicationUser
            {
                Id = "f9852731-c13c-4765-bbb1-9c3321da46ae",
                UserName = "admin",
                Email = "admin@hutech.edu.com",
                FirstName = "Dat",
                LastName = "Dang",
                Gender = true,
                BirthDate = new DateTime(1998,08,18),
                EmailConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                var identityResult = await userManager.CreateAsync(adminUser, "1");
                if (identityResult.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "Administrator"});
                }
            }
        }

    }
}
