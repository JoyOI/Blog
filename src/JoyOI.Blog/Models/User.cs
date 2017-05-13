using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Pomelo.AspNetCore.Extensions.BlobStorage.Models;

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
    }
}
