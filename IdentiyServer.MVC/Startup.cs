using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace IdentiyServer.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //we’ve turned off the JWT claim type mapping to allow well-known claims (e.g. ‘sub’ and ‘idp’) to flow through unmolested

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies"; // We are using a cookie as the primary means to authenticate a user (via "Cookies" as the DefaultScheme)
                options.DefaultChallengeScheme = "oidc"; //We set the DefaultChallengeScheme to "oidc" because when we need the user to login, we will be using the OpenID Connect scheme.
            })
            .AddCookie("Cookies") //We then use AddCookie to add the handler that can process cookies.
            .AddOpenIdConnect("oidc", options => //AddOpenIdConnect is used to configure the handler that perform the OpenID Connect protocol. 
            {
                options.SignInScheme = "Cookies"; //SignInScheme is used to issue a cookie using the cookie handler once the OpenID Connect protocol is complete.

                options.Authority = "http://localhost:5000"; //The Authority indicates that we are trusting IdentityServer. 
                options.RequireHttpsMetadata = false;

                options.ClientId = "mvc"; //We then identify this client via the ClientId
                options.ClientSecret = "secret";
                options.ResponseType = "code id_token";

                options.SaveTokens = true; //SaveTokens is used to persist the tokens from IdentityServer in the cookie
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add("api1");
                options.Scope.Add("brunoArruda");
                options.Scope.Add("offline_access");
            }
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
