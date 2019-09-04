using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Comandante.TestsWeb.Models
{
    public class ComandanteTestsWebContext : IdentityDbContext<IdentityUser>
    {
        public ComandanteTestsWebContext(DbContextOptions<ComandanteTestsWebContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Advert> Adverts { get; set; }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }
        public int Rating { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public DateTime? Published { get; set; }
    }

    public class Advert
    {
        public int Id { get; set; }
        public Blog Blog { get; set; }
        public int? NumberOfDisplays { get; set; }
        public bool? IsActive { get; set; }

        public bool IsDeleted { get; set; }
    }
}
