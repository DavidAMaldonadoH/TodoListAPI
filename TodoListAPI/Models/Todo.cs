using FluentValidation;

namespace TodoListAPI.Models;

public class Todo
{
    public long Id { set; get; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required long UserId { get; set; }
    public User User = null!;
}

public class CreateTodoRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
    }
}

public class UpdateTodoRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
{
    public UpdateTodoRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
    }
}

public class GetTodoResponse
{
    public long Id { set; get; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public class GetAllTodosRequest
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
}

public class GetAllTodosResponse
{
    public List<GetTodoResponse> Data { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
}
