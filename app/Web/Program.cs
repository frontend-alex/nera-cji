namespace nera_cji
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var mvcBuilder = builder.Services.AddControllersWithViews();

            var auth0Section = builder.Configuration.GetSection("Auth0");
            var auth0Domain = auth0Section["Domain"] ?? throw new InvalidOperationException("Missing Auth0 domain configuration.");
            var auth0ClientId = auth0Section["ClientId"] ?? throw new InvalidOperationException("Missing Auth0 client id configuration.");
            var auth0ClientSecret = auth0Section["ClientSecret"] ?? throw new InvalidOperationException("Missing Auth0 client secret configuration.");

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Error";
                    options.ReturnUrlParameter = "returnUrl";
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = $"https://{auth0Domain}";
                    options.ClientId = auth0ClientId;
                    options.ClientSecret = auth0ClientSecret;
                    options.CallbackPath = "/callback";
                    options.ClaimsIssuer = "Auth0";
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            if (context.Properties.Items.TryGetValue("auth0:screen_hint", out var screenHint))
                            {
                                context.ProtocolMessage.SetParameter("screen_hint", screenHint);
                            }

                            return Task.CompletedTask;
                        },
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={auth0ClientId}";
                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    var request = context.Request;
                                    postLogoutUri = $"{request.Scheme}://{request.Host}{request.PathBase}{postLogoutUri}";
                                }

                                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
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

            // App routes (require authentication)
            app.MapControllerRoute(
                name: "app",
                pattern: "app/v1/{controller=Dashboard}/{action=Index}/{id?}");

            // Public routes
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


