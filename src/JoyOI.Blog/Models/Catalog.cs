using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoyOI.Blog.Models
{
    public class Catalog
    {
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(32)]
        public string Url { get; set; }

        public string Title { get; set; }

        public int PRI { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
