using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.Extensions.BlobStorage.Models;

namespace JoyOI.Blog.Models
{
    public class BlogContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IBlobStorageDbContext
    {
        public BlogContext(DbContextOptions<BlogContext> opt)
            : base(opt)
        {
        }

        public DbSet<DomainBinding> DomainBindings { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostTag> PostTags { get; set; }

        public DbSet<Catalog> Catalogs { get; set; }

        public DbSet<Blob> Blobs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.SetupBlobStorage();

            builder.Entity<Catalog>(e =>
            {
                e.HasIndex(x => x.PRI);
            });

            builder.Entity<DomainBinding>(e => 
            {
                e.HasIndex(x => x.Domain);
            });

            builder.Entity<Post>(e =>
            {
                e.HasIndex(x => x.IsPage);
                e.HasIndex(x => x.Time);
                e.HasIndex(x => x.Url).IsUnique();
            });

            builder.Entity<PostTag>(e =>
            {
                e.HasIndex(x => x.Tag);
            });
        }
    }
}
