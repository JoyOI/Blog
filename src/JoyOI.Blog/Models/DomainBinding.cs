using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoyOI.Blog.Models
{
    public class DomainBinding
    {
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(128)]
        public string Domain { get; set; }

        public bool IsDeletable { get; set; }
    }
}
