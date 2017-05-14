using System;
using Microsoft.Extensions.DependencyInjection;
using JoyOI.Blog.Models;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class AdminHelper
    {
        public static bool IsAdmin(this IHtmlHelper self)
        {
            var User = self.ViewContext.HttpContext.RequestServices.GetRequiredService<Identity.SmartUser<User, Guid>>();
            if (User.Current != null && (User.Current.Id == self.ViewBag.OwnerId || User.IsInRole("Root")))
                return true;
            else
                return false;
        }
    }
}
