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
            modelBuilder.Entity<User>(entity => {
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.HasMany(u => u.Shares)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(u => u.Likes)
                    .WithOne(l => l.User)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(u => u.Comments)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(u => u.Following)
                    .WithOne(f => f.User)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(u => u.Followers)
                    .WithOne(f => f.FollowedUser)
                    .HasForeignKey(f => f.FollowedUserId)
                    .OnDelete(DeleteBehavior.NoAction); // Prevent circular cascade delete

                // Configure the JSON conversion for Preferences and Interests
                entity.Property(u => u.Preferences)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>());

                entity.Property(u => u.Interests)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>());
            });
            
            // Content configuration
            modelBuilder.Entity<Content>(entity => {
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                // Configure relationship with Creator (User)
                entity.HasOne(c => c.Creator)
                    .WithMany()
                    .HasForeignKey(c => c.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasMany(c => c.Shares)
                    .WithOne(s => s.Content)
                    .HasForeignKey(s => s.ContentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(c => c.Likes)
                    .WithOne(l => l.Content)
                    .HasForeignKey(l => l.ContentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(c => c.Comments)
                    .WithOne(c => c.Content)
                    .HasForeignKey(c => c.ContentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure the JSON conversion for Categories and Tags
                entity.Property(c => c.Categories)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>());

                entity.Property(c => c.Tags)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>());
            });
            
            // UserInteraction base entity configuration
            // Use TPC (Table-per-Concrete-type) mapping strategy
            modelBuilder.Entity<UserInteraction>()
                .UseTpcMappingStrategy();
                
            // UserInteraction - Set primary key on the base type
            modelBuilder.Entity<UserInteraction>()
                .HasKey(ui => ui.Id);
                
            // UserShare configuration
            modelBuilder.Entity<UserShare>(entity => {
                entity.ToTable("Shares");
                // Don't set key here - it's inherited from the base type
            });
                
            // UserLike configuration
            modelBuilder.Entity<UserLike>(entity => {
                entity.ToTable("Likes");
                // Don't set key here - it's inherited from the base type
            });
                
            // UserComment configuration
            modelBuilder.Entity<UserComment>(entity => {
                entity.ToTable("Comments");
                // Don't set key here - it's inherited from the base type
                
                entity.Property(c => c.CommentText)
                    .IsRequired()
                    .HasMaxLength(1000);
            });
                
            // UserFollow configuration
            modelBuilder.Entity<UserFollow>(entity => {
                entity.ToTable("Follows");
                // Don't set key here - it's inherited from the base type
                
                // Create unique index for follow relationships
                entity.HasIndex(f => new { f.UserId, f.FollowedUserId })
                    .IsUnique();
            });
        }
    }
}