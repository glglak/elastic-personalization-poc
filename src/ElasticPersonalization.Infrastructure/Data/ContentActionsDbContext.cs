using ElasticPersonalization.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElasticPersonalization.Infrastructure.Data
{
    public class ContentActionsDbContext : DbContext
    {
        public ContentActionsDbContext(DbContextOptions<ContentActionsDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Content> Content { get; set; } = null!;
        public DbSet<UserShare> Shares { get; set; } = null!;
        public DbSet<UserLike> Likes { get; set; } = null!;
        public DbSet<UserComment> Comments { get; set; } = null!;
        public DbSet<UserFollow> Follows { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
                
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
                
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Shares)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Following)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.Followers)
                .WithOne(f => f.FollowedUser)
                .HasForeignKey(f => f.FollowedUserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent circular cascade delete
                
            // Content configuration
            modelBuilder.Entity<Content>()
                .HasKey(c => c.Id);
                
            modelBuilder.Entity<Content>()
                .Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            modelBuilder.Entity<Content>()
                .HasMany(c => c.Shares)
                .WithOne(s => s.Content)
                .HasForeignKey(s => s.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Content>()
                .HasMany(c => c.Likes)
                .WithOne(l => l.Content)
                .HasForeignKey(l => l.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Content>()
                .HasMany(c => c.Comments)
                .WithOne(c => c.Content)
                .HasForeignKey(c => c.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Interaction configurations
            modelBuilder.Entity<UserShare>()
                .HasKey(s => s.Id);
                
            modelBuilder.Entity<UserLike>()
                .HasKey(l => l.Id);
                
            modelBuilder.Entity<UserComment>()
                .HasKey(c => c.Id);
                
            modelBuilder.Entity<UserComment>()
                .Property(c => c.CommentText)
                .IsRequired()
                .HasMaxLength(1000);
                
            modelBuilder.Entity<UserFollow>()
                .HasKey(f => f.Id);
                
            // Create unique index for follow relationships
            modelBuilder.Entity<UserFollow>()
                .HasIndex(f => new { f.UserId, f.FollowedUserId })
                .IsUnique();
        }
    }
}
