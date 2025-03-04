using System.Net;
using System.Net.Http.Json;
using TodoListAPI.Models;

namespace TodoListAPI.Tests;

public class AuthController_LoginTests(TodoListApiApplicationFactory factory) : IClassFixture<TodoListApiApplicationFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsBadRequest()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "test@mail.com", Password = "test" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "test@mail.com", Password = "test12345" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("\"Incorrect user or password!\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_ReturnsOk()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "test@mail.com", Password = "test1234" });


        // Assert
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.Token);
    }
}