using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoListAPI.Models;

namespace TodoListAPI.Tests;
public class AuthController_RegisterTests(TodoListApiApplicationFactory factory) : IClassFixture<TodoListApiApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task Register_ReturnsBadRequest()
    {
        // Arrange
        var invalidUser = new CreateUserRequest
        {
            Name = "",
            Email = "",
            Password = ""
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/register",
            invalidUser
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(3, problemDetails.Errors.Count);
        Assert.Contains("Name", problemDetails.Errors.Keys);
        Assert.Contains("Email", problemDetails.Errors.Keys);
        Assert.Contains("Password", problemDetails.Errors.Keys);
        Assert.Contains("'Name' must not be empty.", problemDetails.Errors["Name"]);
        Assert.Contains("'Email' must not be empty.", problemDetails.Errors["Email"]);
        Assert.Contains("'Password' must not be empty.", problemDetails.Errors["Password"]);
    }

    [Fact]
    public async Task Register_ReturnsCreatedResult()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/register",
            new CreateUserRequest
            {
                Name = "Register Test",
                Email = "register@mail.com",
                Password = "test1234"
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}