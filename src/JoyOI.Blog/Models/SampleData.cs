using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JoyOI.Blog.Models
{
    public static class SampleData
    {
        public static async Task InitializeYuukoBlog(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<BlogContext>();
            var roleManager = serviceProvider.GetService<RoleManager<Role>>();
            await db.Database.EnsureCreatedAsync();
            await roleManager.CreateAsync(new Role("Root"));
        }
    }
}
