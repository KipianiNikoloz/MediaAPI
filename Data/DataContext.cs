using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions options): base(options) {}

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> UserLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.LikedUserId});

            modelBuilder.Entity<UserLike>().HasOne(u => u.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(u => u.SourceUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserLike>()
                .HasOne(u => u.LikedUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(u => u.LikedUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}