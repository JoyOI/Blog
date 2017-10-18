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
                .Where(x => x.ProblemId == id);

            var count = await query.CountAsync(token);

            var posts = await query
                .OrderByDescending(x => x.Time)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    time = x.Time,
                    userId = x.User.OpenId,
                    username = x.User.UserName,
                    avatarUrl = x.User.AvatarUrl
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
