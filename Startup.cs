using BugTracker.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Online_market.Data;
using Online_market.Models;
using Online_market.Services;
using Online_market.Utilities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market
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
                options.UseNpgsql(
                    DataManage.GetConnectionString(Configuration)));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<CustomUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
             .AddDefaultUI()
             .AddDefaultTokenProviders()
             .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddMvc().AddNewtonsoftJson();
            services.AddScoped<IImageService, ImageService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IHttpConnectionFeature, HttpConnectionFeature>();
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            services.AddScoped<IEmailSender, EmailService>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUserDetector, UserDetector>();
            services.AddScoped<ICreateTemporaryUser, CreateTemporaryUser>();
            services.AddScoped<ISlugService, SlugService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StripeConfiguration.ApiKey = "sk_test_51IVu6KI523rItHV0dXeuri27Y4bo8mhWCusna1obgrLwimMJYFxC8WjG4v7FeTwWnXqX0ezQn9fDnqENw2I1p3Hl00Oe8YO2e5";

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
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
                   name: "slugRoute",
                   pattern: "Items/Details/{slug}",
                   defaults: new { Controller = "Items", action = "Details" });
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                  name: "slugRoute2",
                  pattern: "Category/{slug}",
                  defaults: new { Controller = "Categories", action = "CategoryIndex" });
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
