using AuthSample.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AuthSample
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
            .AddWsFederation(options =>
            {
                // MetadataAddress represents the Active Directory instance used to authenticate users.
                //<CHANGE REQUIRED> tenant ID
                options.MetadataAddress = "https://login.microsoftonline.com/<TENANT ID>/federationmetadata/2007-06/federationmetadata.xml";

                // Wtrealm is the app's identifier in the Active Directory instance.
                // For ADFS, use the relying party's identifier, its WS-Federation Passive protocol URL:
                options.Wtrealm = "https://localhost:44398/";

                // For AAD, use the Application ID URI from the app registration's Overview blade:
                options.Wtrealm = "api://b464800a-f756-4688-8c6c-eaf46ae9dc0a";
            })
            .AddSaml2(options=>
            {
                //<CHANGE REQUIRED> entity id
                options.SPOptions.EntityId = new EntityId("https://CONTOSO.onmicrosoft.com/customappsso2/primary");
                options.IdentityProviders.Add(
                  new IdentityProvider(
                    //<CHANGE REQUIRED> tenant id
                    new EntityId("https://sts.windows.net/<TENANT ID>/"), options.SPOptions)
                  {
                      //<CHANGE REQUIRED> tenant id & app id
                      MetadataLocation = "https://login.microsoftonline.com/<TENANT ID>/federationmetadata/2007-06/federationmetadata.xml?appid=<APP ID>"
                  });
            })
            .AddCookie();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
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
                endpoints.MapRazorPages();
            });
        }
    }
}
