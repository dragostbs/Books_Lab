using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dragos_Boscan_L2.Data;
using Microsoft.EntityFrameworkCore;
using Dragos_Boscan_L2.Hubs;
using Microsoft.AspNetCore.Identity;

namespace Dragos_Boscan_L2
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
            services.AddControllersWithViews();
            services.AddDbContext<LibraryContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSignalR();
            services.AddRazorPages();

            services.Configure<IdentityOptions>(options => {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); 
                options.Lockout.MaxFailedAccessAttempts = 3; 
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequiredLength = 8;
            });

            services.AddAuthorization(opts => {
                opts.AddPolicy("OnlySales", policy => {
                    policy.RequireClaim("Department", "Sales");
                });
            });

            services.AddAuthorization(opts => {
                opts.AddPolicy("SalesManager", policy => {
                    policy.RequireRole("Manager");
                    policy.RequireClaim("Department", "Sales");
                });
            });

            services.ConfigureApplicationCookie(opts =>
            {
                opts.AccessDeniedPath = "/Identity/Account/AccessDenied";

            });
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
                endpoints.MapHub<ChatHub>("/chathub");
                endpoints.MapRazorPages();
            });
        }
    }
}
