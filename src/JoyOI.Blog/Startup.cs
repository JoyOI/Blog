using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.Localization;
using JoyOI.Blog.Models;

namespace JoyOI.Blog
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Configuration;
            services.AddConfiguration(out Configuration);

            services.AddDbContext<BlogContext>(x => 
            {
                x.UseMySql(Configuration["Data:MySql"]);
                x.UseMySqlLolita();
            });

            services.AddSmartCookies();

            services.AddMemoryCache();

            services.AddIdentity<User, IdentityRole<Guid>>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<BlogContext, Guid>()
                .AddDefaultTokenProviders();

            services.AddBlobStorage()
                .AddEntityFrameworkStorage<BlogContext>()
                .AddSessionUploadAuthorization();

            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
            });

            services.AddMvc()
                .AddMultiTemplateEngine()
                .AddCookieTemplateProvider();

            services.AddSmartCookies();
            services.AddSmartUser<User, Guid>();

            services.AddJoyOIUserCenter();
        }
        
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Warning, true);

            app.UseStaticFiles();
            app.UseIdentity();
            app.UseBlobStorage("/assets/shared/scripts/jquery.codecomb.fileupload.js");
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            await SampleData.InitializeYuukoBlog(app.ApplicationServices);
        }
    }
}
