using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.Blog.Controllers
{
    public class PostController : BaseController
    {
        [Route("Post/{id}")]
        public IActionResult Post(string id)
        {
            var post = DB.Posts
                .Include(x => x.Catalog)
                .Include(x => x.Tags)
                .Where(x => x.UserId == SiteOwnerId)
                .Where(x => x.Url == id && !x.IsPage)
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
            ViewBag.Title = post.Title;
            ViewBag.Position = post.CatalogId != null ? post.Catalog.Url : "home";
            return View(post);
        }

        [Route("{id:regex(^([[a-zA-Z_-]]{{1,64}}[[a-zA-Z0-9_-]]{{0,64}})$)}")]
        public IActionResult Page(string id)
        {
            var post = DB.Posts
                .Where(x => x.UserId == SiteOwnerId)
                .Where(x => x.Url == id && x.IsPage)
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
            ViewBag.Title = post.Title;
            ViewBag.Position = post.CatalogId.HasValue ? post.Catalog.Url : "home";
            return View("Post", post);
        }
    }
}
