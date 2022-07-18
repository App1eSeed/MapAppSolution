using MapApp.Models.EF;
using MapApp.Models.QueryModels;
using MapApp.Services.MailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers().AddJsonOptions(x =>
            //    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

            services.AddDbContext<MapAppContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddControllersWithViews();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // options.DefaultChallengeScheme = "okta";

            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            options =>
            {
                //Open Modal
                options.LoginPath = "/Account/LoginReroute";
                options.AccessDeniedPath = "/Home/Index";
                
            })
            .AddOpenIdConnect("okta", options =>
            {
                options.Authority = "https://dev-6385717.okta.com";
                options.ClientId = "0oa55awb7kjSH5Ysu5d7";
                options.ClientSecret = "qd0CBl6x2VnJzv3KcVsrxVswpSIcWO-DvD-vA0Rj";
                options.CallbackPath = "/okta-Auth";

//                options.SignedOutCallbackPath = new PathString("http://localhost:44349");
                options.ResponseType = "code";

                options.GetClaimsFromUserInfoEndpoint = true;//!!



                //options.SaveTokens = false;
                //options.Scope.Add("openid");
                //options.Scope.Add("profile");
                //options.Events = new OpenIdConnectEvents()
                //{
                //    OnRedirectToIdentityProvider = async (context) =>
                //    {
                //        var redirectUri = context.ProtocolMessage.RedirectUri;
                //        await Task.CompletedTask;
                //    }
                //};
            });
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            services.AddMemoryCache();

            services.AddTransient<IMailService, MailService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
