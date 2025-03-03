using Microsoft.EntityFrameworkCore;
using TodoListAPI.Models;

namespace TodoListAPI;

public class TodoListDbContext(DbContextOptions<TodoListDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<Todo> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("User");

            user.Property(u => u.Id).HasColumnType("bigint");
            user.HasKey(u => u.Id);
            user.Property(u => u.Name).IsRequired().HasColumnType("varchar(128)");
            user.Property(u => u.Email).IsRequired().HasColumnType("varchar(128)");
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Password).IsRequired().HasColumnType("varchar(255)");
        });

        modelBuilder.Entity<Todo>(todo =>
        {
            todo.ToTable("Todo");

            todo.Property(t => t.Id).HasColumnType("bigint");
            todo.HasKey(t => t.Id);
            todo.Property(t => t.Title).HasColumnType("varchar(128)");
            todo.Property(t => t.Description).HasColumnType("varchar(255)");
            todo.Property(t => t.UserId).HasColumnType("bigint");
        });

        modelBuilder.Entity<Todo>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("FK_user_todos")
            .IsRequired();
    }
}