using System;
using Microsoft.AspNetCore.Identity;

namespace JoyOI.Blog.Models
{
    public class Role : IdentityRole<Guid>
    {
        public Role() { }

        public Role(string role) : base(role) { }
    }
}
