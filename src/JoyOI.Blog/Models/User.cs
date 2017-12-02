using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JoyOI.Blog.Models
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(64)]
        public string SiteName { get; set; }

        [MaxLength(256)]
        public string Summary { get; set; }

        [MaxLength(64)]
        public string Nickname { get; set; }

        [MaxLength(32)]
        public string Template { get; set; }

        [MaxLength(64)]
        public string AccessToken { get; set; }

        public DateTime ExpireTime { get; set; }

        public Guid OpenId { get; set; }

        [MaxLength(256)]
        public string AvatarUrl { get; set; }

        public virtual ICollection<DomainBinding> Domains { get; set; } = new List<DomainBinding>();
    }
}
