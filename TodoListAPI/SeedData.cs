using Microsoft.EntityFrameworkCore;
using TodoListAPI.Models;

namespace TodoListAPI;

public static class SeedData
{
    public static void SeedAndMigrate(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<TodoListDbContext>();
        context.Database.Migrate();

        if (!context.Users.Any())
        {
            context.Add(
                new User
                {
                    Name = "Test User",
                    Email = "test@mail.com",
                    Password = PasswordHelper.HashPassword("test1234")
                }
            );
        }

        context.SaveChanges();

        if (!context.Todos.Any())
        {
            context.AddRange(
                new Todo
                {
                    Title = "Test Todo",
                    Description = "Test Description",
                    UserId = 1
                },
                new Todo
                {
                    Title = "Test Todo 2",
                    Description = "Test Description 2",
                    UserId = 1
                },
                new Todo
                {
                    Title = "Test Todo 3",
                    Description = "Test Description 3",
                    UserId = 1
                },
                new Todo
                {
                    Title = "Test Todo 4",
                    Description = "Test Description 4",
                    UserId = 1
                },
                new Todo
                {
                    Title = "Test Todo 5",
                    Description = "Test Description 5",
                    UserId = 1
                },
                new Todo
                {
                    Title = "Test Todo 6",
                    Description = "Test Description 6",
                    UserId = 1
                }
            );
        }

        context.SaveChanges();
    }
}