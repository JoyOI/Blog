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

        [ForeignKey("Avatar")]
        public Guid AvatarId { get; set; }

        public virtual Blob Avatar { get; set; }
    }
}
