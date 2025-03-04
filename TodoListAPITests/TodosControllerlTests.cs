using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoListAPI.Models;

namespace TodoListAPI.Tests;

public class TodosControllerTest(TodoListApiApplicationFactory factory) : IClassFixture<TodoListApiApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task GetTodos_ReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/todos?page=1&limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_ReturnsOk()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.GetAsync("/api/todos?page=2&limit=5");
        var getAllTodosResponse = await response.Content.ReadFromJsonAsync<GetAllTodosResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(getAllTodosResponse);
        Assert.NotEmpty(getAllTodosResponse.Data);
        Assert.Single(getAllTodosResponse.Data);
        Assert.Equal(2, getAllTodosResponse.Page);
        Assert.Equal(5, getAllTodosResponse.Limit);
        Assert.Equal(6, getAllTodosResponse.Total);
    }

    [Fact]
    public async Task GetTodo_RetunrsUnauthorized()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/todos/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTodo_ReturnNotFound()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.GetAsync("/api/todos/100");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTodo_ReturnsOk()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.GetAsync("/api/todos/1");
        var getTodoResponse = await response.Content.ReadFromJsonAsync<GetTodoResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(getTodoResponse);
        Assert.Equal(1, getTodoResponse.Id);
        Assert.Equal("Test Todo", getTodoResponse.Title);
        Assert.Equal("Test Description", getTodoResponse.Description);
        Assert.False(getTodoResponse.IsCompleted);
    }

    [Fact]
    public async Task CreateTodo_ReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/todos", new CreateTodoRequest
        {
            Title = "Test Todo",
            Description = "Test Description"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_ReturnsBadRequest()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/todos", new CreateTodoRequest
        {
            Title = "",
            Description = ""
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Title", problemDetails.Errors.Keys);
        Assert.Contains("Description", problemDetails.Errors.Keys);
        Assert.Contains("'Title' must not be empty.", problemDetails.Errors["Title"]);
        Assert.Contains("'Description' must not be empty.", problemDetails.Errors["Description"]);
    }

    [Fact]
    public async Task CreateTodo_ReturnsCreated()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/todos", new CreateTodoRequest
        {
            Title = "New Test Todo",
            Description = "Test Description"
        });

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateTodo_ReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.PutAsJsonAsync("/api/todos/1", new UpdateTodoRequest
        {
            Title = "Test Todo",
            Description = "Test Description"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_ReturnsOk()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.PutAsJsonAsync("/api/todos/2", new UpdateTodoRequest
        {
            Title = "Updated Test Todo",
            Description = "Updated Test Description"
        });

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Failed to update todo: {content}");
        }
        // Assert
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoListDbContext>();
        var todo = await dbContext.Todos.SingleOrDefaultAsync(t => t.Id == 2);

        Assert.NotNull(todo);
        Assert.Equal("Updated Test Todo", todo.Title);
        Assert.Equal("Updated Test Description", todo.Description);
    }

    [Fact]
    public async Task UpdateTodo_MarksAsCompleted()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.PatchAsJsonAsync("/api/todos/1", new PatchTodoRequest
        {
            IsCompleted = true
        });

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Failed to update todo: {content}");
        }
        // Assert
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoListDbContext>();
        var todo = await dbContext.Todos.SingleOrDefaultAsync(t => t.Id == 1);

        Assert.NotNull(todo);
        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNotFound()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);

        // Act
        var response = await _httpClient.DeleteAsync("/api/todos/100");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNoContent()
    {
        // Arrange
        await AddAuthorizationToClientForRoleAsync(_httpClient);
        var newTodo = new Todo
        {
            Title = "New Test Todo",
            Description = "Test Description",
            UserId = 1
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoListDbContext>();
            await dbContext.AddAsync(newTodo);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _httpClient.DeleteAsync($"/api/todos/{newTodo.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    protected static async Task AddAuthorizationToClientForRoleAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "test@mail.com",
            Password = "test1234"
        });

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
    }
}