namespace nera_cji
{
    using App.Core;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using nera_cji.Infrastructure.Services;
    using nera_cji.Interfaces.Services;
    using nera_cji.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            var mvcBuilder = builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IEventRegistrationService, EventRegistrationService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
            builder.Services.AddScoped<nera_cji.Interfaces.Services.IAuth0Service, nera_cji.Services.Auth0Service>();
             builder.Services.AddScoped<nera_cji.Interfaces.Services.IEventService, nera_cji.Services.EventService>();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Error";
                    options.ReturnUrlParameter = "returnUrl";
                });
            
            if (builder.Environment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "app",
                pattern: "app/v1/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "contact",
                pattern: "contact",
                defaults: new { controller = "Home", action = "Contact" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}


