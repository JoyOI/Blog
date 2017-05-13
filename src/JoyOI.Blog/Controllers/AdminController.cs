﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.Marked;
using JoyOI.Blog.Models;
using JoyOI.UserCenter.SDK;

namespace JoyOI.Blog.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            return View(SiteOwner);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string site, string summary)
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            SiteOwner.SiteName = site;
            SiteOwner.Summary = summary;
            DB.SaveChanges();

            return Prompt(x => 
            {
                x.Title = "Succeeded";
                x.Details = SR["Saved successfully."];
            });
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string Username, 
            string Password,
            [FromServices] JoyOIUC UC)
        {
            var authorizeResult = await UC.TrustedAuthorizeAsync(Username, Password);
            if (authorizeResult.succeeded)
            {
                var profileResult = await UC.GetUserProfileAsync(authorizeResult.data.open_id, authorizeResult.data.access_token);
                var user = new User
                {
                    UserName = Username,
                    Email = profileResult.data.email,
                    PhoneNumber = profileResult.data.phone,
                    SiteName = profileResult.data.nickname,
                    Template = "Default",
                    AccessToken = authorizeResult.data.access_token,
                    ExpireTime = authorizeResult.data.expire_time,
                    OpenId = authorizeResult.data.open_id,
                    AvatarUrl = UC.GetAvatarUrl(authorizeResult.data.open_id)
                };

                await UserManager.CreateAsync(user, Password);

                if (authorizeResult.data.is_root)
                {
                    await UserManager.AddToRoleAsync(user, "Root");
                }

                await SignInManager.SignInAsync(new User { UserName = Username }, false);

                return RedirectToAction("Index");
            }
            else
            {
                return Prompt(x =>
                {
                    x.Title = SR["Sign in failed"];
                    x.Details = authorizeResult.msg;
                    x.StatusCode = authorizeResult.code;
                });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Post/Edit")]
        public async Task<IActionResult> PostEdit(string id, string newId, string tags, bool isPage, string title, Guid? catalog, string content)
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var post = DB.Posts
                .Include(x => x.Tags)
                .Where(x => x.UserId == SiteOwner.Id)
                .Where(x => x.Url == id)
                .SingleOrDefault();
            if (post == null)
                return Prompt(x =>
                {
                    x.StatusCode = 404;
                    x.Title = SR["Not Found"];
                    x.Details = SR["The resources have not been found, please check your request."];
                    x.RedirectUrl = Url.Link("default", new { controller = "Home", action = "Index" });
                    x.RedirectText = SR["Back to home"];
                });
            var summary = "";
            var flag = false;
            if (content != null)
            {
                var tmp = content.Split('\n');
                if (tmp.Count() > 16)
                {
                    for (var i = 0; i < 16; i++)
                    {
                        if (tmp[i].IndexOf("```") == 0)
                            flag = !flag;
                        summary += tmp[i] + '\n';
                    }
                    if (flag)
                        summary += "```\r\n";
                    summary += $"\r\n[{SR["Read More"]} »](/post/{newId})";
                }
                else
                {
                    summary = content;
                }
            }
            foreach (var t in post.Tags)
                DB.PostTags.Remove(t);
            post.Url = newId;
            post.Summary = summary;
            post.Title = title;
            post.Content = content;
            post.CatalogId = catalog;
            post.IsPage = isPage;
            if (!string.IsNullOrEmpty(tags))
            { 
                var _tags = tags.Split(',');
                foreach (var t in _tags)
                    post.Tags.Add(new PostTag { PostId = post.Id, Tag = t.Trim(' ') });
            }
            DB.SaveChanges();
            return Content(Instance.Parse(content));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Post/Delete")]
        public async Task<IActionResult> PostDelete(string id)
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var post = DB.Posts
                .Include(x => x.Tags)
                .Where(x => x.UserId == SiteOwner.Id)
                .Where(x => x.Url == id).SingleOrDefault();
            
            if (post == null)
                return Prompt(x =>
                {
                    x.StatusCode = 404;
                    x.Title = SR["Not Found"];
                    x.Details = SR["The resources have not been found, please check your request."];
                    x.RedirectUrl = Url.Link("default", new { controller = "Home", action = "Index" });
                    x.RedirectText = SR["Back to home"];
                });
            foreach (var t in post.Tags)
                DB.PostTags.Remove(t);
            DB.Posts.Remove(post);
            DB.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Post/New")]
        public async Task<IActionResult> PostNew()
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Url = Guid.NewGuid().ToString().Substring(0, 8),
                Title = SR["Untitled Post"],
                Content = "",
                Summary = "",
                CatalogId = null,
                IsPage = false,
                Time = DateTime.Now
            };
            DB.Posts.Add(post);
            DB.SaveChanges();
            return RedirectToAction("Post", "Post", new { id = post.Url });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        public async Task<IActionResult> Catalog()
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            return View(DB.Catalogs
                .Where(x => x.UserId == SiteOwner.Id)
                .OrderByDescending(x => x.PRI)
                .ToList());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Catalog/Delete")]
        public async Task<IActionResult> CatalogDelete(string id)
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var catalog = DB.Catalogs
                .Where(x => x.UserId == SiteOwner.Id)
                .Where(x => x.Url == id)
                .SingleOrDefault();
            if (catalog == null)
                return Prompt(x =>
                {
                    x.StatusCode = 404;
                    x.Title = SR["Not Found"];
                    x.Details = SR["The resources have not been found, please check your request."];
                    x.RedirectUrl = Url.Link("default", new { controller = "Home", action = "Index" });
                    x.RedirectText = SR["Back to home"];
                });
            DB.Catalogs.Remove(catalog);
            DB.SaveChanges();
            return Content("true");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Catalog/Edit")]
        public async Task<IActionResult> CatalogEdit(string id, string newId, string title, int pri)
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var catalog = DB.Catalogs
                .Where(x => x.UserId == SiteOwner.Id)
                .Where(x => x.Url == id)
                .SingleOrDefault();
            if (catalog == null)
                return Prompt(x =>
                {
                    x.StatusCode = 404;
                    x.Title = SR["Not Found"];
                    x.Details = SR["The resources have not been found, please check your request."];
                    x.RedirectUrl = Url.Link("default", new { controller = "Home", action = "Index" });
                    x.RedirectText = SR["Back to home"];
                });
            catalog.Url = newId;
            catalog.Title = title;
            catalog.PRI = pri;
            DB.SaveChanges();
            return Content("true");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Catalog/New")]
        public async Task<IActionResult> CatalogNew()
        {
            if (SiteOwner.Id != User.Current.Id && (await UserManager.GetRolesAsync(User.Current)).Any(x => x == "Root"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Access Denied"];
                    x.Details = SR["You don't have the permission to access this page."];
                    x.StatusCode = 403;
                });
            }

            var catalog = new Catalog
            {
                Url = Guid.NewGuid().ToString().Substring(0, 8),
                PRI = 0,
                Title = SR["New Catalog"]
            };
            DB.Catalogs.Add(catalog);
            DB.SaveChanges();
            return RedirectToAction("Catalog", "Admin");
        }
    }
}