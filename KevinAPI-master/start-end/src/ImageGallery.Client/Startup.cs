using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using IdentityModel;

namespace ImageGallery.Client
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // To get same claim type as we defined. format claim
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            //Adding Policy step 1
            services.AddAuthorization(
                authOptions => {
                    authOptions.AddPolicy(
                        "CanOrderFrame",
                        policybuilder =>
                        {
                            policybuilder.RequireAuthenticatedUser();
                            policybuilder.RequireClaim("country","be");
                            policybuilder.RequireClaim("subscriptionlevel", "PayingUser");
                        }
                        );
                }
                );

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            
            services.AddAuthentication(
                    option =>
                    {
                        option.DefaultScheme = "Cookies"; // Not strictly req.
                        option.DefaultChallengeScheme = "oidc"; // This will match the scheme we use to configure oidc
                    }
                ).AddCookie("Cookies", (option) => { option.AccessDeniedPath = "/Authorization/AccessDenied"; }) //This configures cookie handler and 
                                                                                                                //it enables our application to use cookie based authentication for our default scheme.
                                                                                                                //Once an identity token is validated and transformed into a claims identity, it will be stored in an encrypted cookie which is then used 
                                                                                                                //on subsequent requests to the web app.
                                                                                                                //option.AccessDeniedPath = "Authorization/AccessDenied" : Redirects if Authentication fails.


                .AddOpenIdConnect("oidc", options => //To register and configure the oidc handler.
                {
                    options.SignInScheme = "Cookies";
                    options.Authority = "https://localhost:44377/";
                    options.ClientId = "imagegalleryclient";
                    options.ResponseType = "code id_token";
                    //options.CallbackPath = new PathString("...")
                    //options.SignedOutCallbackPath = new PathString("...")
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");
                    options.Scope.Add("imagegalleryapi");

                    options.Scope.Add("country");
                    options.Scope.Add("subscriptionlevel");

                    options.SaveTokens = true;
                    options.ClientSecret = "secret";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //get claim which was absent
                    options.ClaimActions.Remove("amr");

                    //Remove unnecessary claim from middleware
                    options.ClaimActions.DeleteClaim("idp");
                    options.ClaimActions.DeleteClaim("sid");

                    options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                    options.ClaimActions.MapUniqueJsonKey("country", "country");

                    // To add Role in User object.
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        NameClaimType = JwtClaimTypes.GivenName,
                        RoleClaimType = JwtClaimTypes.Role
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication(); //Adding authentication middleware
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }         
    }
}
