using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TodoListAPI.Models;

namespace TodoListAPI.Controllers;

public class AuthController(TodoListDbContext dbContext, IConfiguration configuration) : BaseController
{
    private readonly TodoListDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="request">Contains the email and password</param>
    /// <returns>JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest();
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            return Unauthorized("Incorrect user or password!");
        }

        var token = GenerateJwtToken(new GetUserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
        });

        return Ok(new LoginResponse { Token = token });
    }

    /// <summary>
    /// Register
    /// </summary>
    /// <param name="request">The user to be created</param>
    /// <returns>A link to the user that was created</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var userEmail = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (userEmail != null)
        {
            return BadRequest("Email already in use");
        }


        var newUser = new User
        {
            Name = request.Name!,
            Email = request.Email!,
            Password = PasswordHelper.HashPassword(request.Password!)
        };

        try
        {
            await _dbContext.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            return StatusCode(201, newUser);
        }
        catch
        {
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    private string GenerateJwtToken(GetUserResponse user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "test_jwt_key"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}