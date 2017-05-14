using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JoyOI.Blog.Models;

namespace JoyOI.Blog.Controllers
{
    public class BaseController : BaseController<BlogContext, User, Guid>
    {
        public User SiteOwner { get; private set; }

        public override void Prepare()
        {
            base.Prepare();

            SiteOwner = DB.DomainBindings
                .Include(x => x.User)
                .SingleOrDefault(x => x.Domain == HttpContext.Request.Host.Host)
                ?.User;

            if (SiteOwner == null && !Request.Path.Value.ToLower().StartsWith("/admin"))
            {
                HttpContext.Response.Redirect("http://www.joyoi.net");
                return;
            }

            // Building Constants
            ViewBag.Position = "home";
            ViewBag.IsPost = false;
            ViewBag.Description = SiteOwner?.Summary;
            ViewBag.Title = SiteOwner?.SiteName;
            ViewBag.AboutUrl = "/about";
            ViewBag.AvatarUrl = SiteOwner?.AvatarUrl;
            ViewBag.Account = SiteOwner?.Nickname;
            ViewBag.DefaultTemplate = SiteOwner?.Template;

            var siteOwnerId = SiteOwner?.Id;
            ViewBag.OwnerId = SiteOwner == null ? null : (Guid?)SiteOwner.Id;

            // Building Tags
            ViewBag.Tags = DB.PostTags
                .Where(x => x.Post.UserId == siteOwnerId)
                .OrderBy(x => x.Tag)
                .GroupBy(x => x.Tag)
                .Select(x => new TagViewModel
                {
                    Title = x.Key,
                    Count = x.Count()
                })
                .ToList();

            // Building Calendar
            ViewBag.Calendars = DB.Posts
                .Where(x => x.UserId == siteOwnerId)
                .Where(x => !x.IsPage)
                .OrderByDescending(x => x.Time)
                .GroupBy(x => new { Year = x.Time.Year, Month = x.Time.Month })
                .Select(x => new CalendarViewModel
                {
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Count = x.Count()
                })
                .ToList();

            // Building Catalogs
            ViewBag.Catalogs = DB.Catalogs
                .Include(x => x.Posts)
                .Where(x => x.UserId == siteOwnerId)
                .OrderByDescending(x => x.PRI)
                .ToList()
                .Select(x => new CatalogViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Count = x.Posts.Count(),
                    PRI = x.PRI,
                    Url = x.Url
                })
                .ToList();
        }
    }
}
