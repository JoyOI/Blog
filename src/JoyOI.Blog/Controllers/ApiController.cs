using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.Blog.Controllers
{
    public class ApiController : BaseController
    {
        [Route("Api/Resolution/{id}")]
        public async Task<IActionResult> Resolution(string id, int? page, CancellationToken token)
        {
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException("problem id could not be null");

            if (!page.HasValue)
                page = 1;

            var query = DB.Posts
                .Include(x => x.User)
                .ThenInclude(x => x.Domains)
                .Include(x => x.Tags)
                .Where(x => x.ProblemId == id)
                .Where(x => x.Content.Length > 0);

            var count = await query.CountAsync(token);

            var posts = (await query
                .OrderByDescending(x => x.Time)
                .Skip((page.Value - 1) * 20)
                .Take(20)
                .ToListAsync(token))
                .Select(x => new
                {
                    id = x.Id,
                    url = x.Url,
                    title = x.Title,
                    time = x.Time,
                    userId = x.User.OpenId,
                    username = x.User.UserName,
                    avatarUrl = x.User.AvatarUrl,
                    domain = x.User.Domains.ToList().Last().Domain,
                    tags = x.Tags.Select(y => y.Tag)
                });

            return Json(new
            {
                pageSize = 20,
                pageCount = (count + 20 - 1) / 20,
                total = count,
                data = posts
            });
        }

        [Route("Api/BlogDomain/{id}")]
        public async Task<IActionResult> BlogDomain(string id, CancellationToken token)
        {
            var domain = await DB.DomainBindings
                .Where(x => x.User.UserName == id)
                .OrderByDescending(x => x.IsDeletable)
                .FirstAsync(token);

            return Content(domain.Domain);
        }

        [HttpGet]
        public async Task<IActionResult> UserResolution(string id, int? page, CancellationToken token)
        {
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException("problem id could not be null");

            if (!page.HasValue)
                page = 1;

            var user = DB.Users
                .Include(x => x.Domains)
                .Where(x => x.UserName == id)
                .SingleOrDefault();

            var domain = user.Domains.Last().Domain;

            var query = DB.Posts
                .Include(x => x.User)
                .Include(x => x.Tags)
                .Where(x => x.User.UserName == id)
                .Where(x => !string.IsNullOrEmpty(x.ProblemId))
                .Where(x => x.Content.Length > 0);

            var count = await query.CountAsync(token);

            var posts = await query
                .OrderByDescending(x => x.Time)
                .Select(x => new
                {
                    id = x.Id,
                    url = x.Url,
                    title = x.Title,
                    time = x.Time,
                    problemId = x.ProblemId,
                    problemTitle = x.ProblemTitle,
                    domain = domain
                })
                .Skip((page.Value - 1) * 20)
                .Take(20)
                .ToListAsync(token);

            return Json(new
            {
                pageSize = 20,
                pageCount = (count + 20 - 1) / 20,
                total = count,
                data = posts
            });
        }
    }
}
