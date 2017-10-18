using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JoyOI.Blog.Models;
using System.Collections.Generic;

namespace JoyOI.Blog.Controllers
{
    public class BaseController : BaseController<BlogContext, User, Guid>
    {
        public User SiteOwner { get; private set; }

        public Guid? SiteOwnerId { get; private set; }

        public override void Prepare()
        {
            base.Prepare();

            SiteOwner = DB.DomainBindings
                .Include(x => x.User)
                .SingleOrDefault(x => x.Domain == HttpContext.Request.Host.Host)
                ?.User;

            if (SiteOwner == null && !Request.Path.Value.ToLower().StartsWith("/admin") && !Request.Path.Value.ToLower().StartsWith("/api"))
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

            SiteOwnerId = SiteOwner?.Id;
            ViewBag.OwnerId = SiteOwner == null ? null : (Guid?)SiteOwner.Id;
            ViewBag.Owner = SiteOwner;

            // Building Tags
            var tags = new List<TagViewModel>();
            if (SiteOwner != null)
            {
                tags = DB.PostTags
                .Where(x => x.Post.UserId == SiteOwnerId)
                .OrderBy(x => x.Tag)
                .GroupBy(x => x.Tag)
                .Select(x => new TagViewModel
                {
                    Title = x.Key,
                    Count = x.Count()
                })
                .ToList();
            }
            ViewBag.Tags = tags;

            // Building Calendar
            var calendars = new List<CalendarViewModel>();
            if (SiteOwner != null)
            {
                calendars = DB.Posts
                .Where(x => x.UserId == SiteOwnerId)
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
            }
            ViewBag.Calendars = calendars;

            // Building Catalogs
            var catalogs = new List<CatalogViewModel>();
            if (SiteOwner != null)
            {
                catalogs = DB.Catalogs
                .Include(x => x.Posts)
                .Where(x => x.UserId == SiteOwnerId)
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
            ViewBag.Catalogs = catalogs;
        }
    }
}
