using FluentValidation;

namespace TodoListAPI.Models;

public class User
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public List<Todo> Todos = [];
}

public class CreateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(128);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class GetUserResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(128);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class LoginResponse
{
    public required string Token { get; set; }
}