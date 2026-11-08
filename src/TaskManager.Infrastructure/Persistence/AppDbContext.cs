using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private const int MaxEmailLength = 320;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserRecord> Users => Set<UserRecord>();

    public DbSet<TaskRecord> Tasks => Set<TaskRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(MaxEmailLength);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<TaskRecord>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.OwnerId).IsRequired();
            entity.Property(task => task.Title).IsRequired().HasMaxLength(TaskItem.MaxTitleLength);
            entity.Property(task => task.Description).IsRequired().HasMaxLength(TaskItem.MaxDescriptionLength);
            entity.Property(task => task.Status).IsRequired().HasMaxLength(32);
            entity.Property(task => task.DueDate).IsRequired();
            entity.HasIndex(task => task.OwnerId);
            entity.HasOne<UserRecord>()
                .WithMany()
                .HasForeignKey(task => task.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
