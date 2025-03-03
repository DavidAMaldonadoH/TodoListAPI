using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListAPI.Models;

namespace TodoListAPI.Controllers;

public class TodosController(TodoListDbContext dbContext) : BaseController
{
    private readonly TodoListDbContext _dbContext = dbContext;

    /// <summary>
    /// Get all TODOs
    /// </summary>
    /// <param name="request">Filters to the array of todos</param>
    /// <returns>An array of the filtered todos</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(GetAllTodosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllTodosRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");

        var page = request?.Page ?? 1;
        var limit = request?.Limit ?? 10;

        var existingUser = await _dbContext.Users.FindAsync(userId);
        if (existingUser == null)
        {
            return BadRequest();
        }

        IQueryable<Todo> query = _dbContext.Todos
            .Skip((page - 1) * limit)
            .Take(limit)
            .Where(t => t.UserId == existingUser.Id);

        var todos = await query.ToArrayAsync();

        var allTodos = await _dbContext.Todos.Where(t => t.UserId == existingUser.Id).ToArrayAsync();

        return Ok(new GetAllTodosResponse
        {
            Data = [.. todos.Select(TodoToGetTodoResponse)],
            Limit = limit,
            Page = page,
            Total = allTodos.Length
        });
    }

    /// <summary>
    /// Get a TODO by its ID
    /// </summary>
    /// <param name="id">The ID of the TODO</param>
    /// <returns>The single TODO record</returns>
    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(GetTodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodoById([FromRoute] long id)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var existingUser = await _dbContext.Users.FindAsync(userId);
        if (existingUser == null)
        {
            return NotFound();
        }

        var existingTodo = await _dbContext.Todos.SingleOrDefaultAsync(t => t.Id == id);
        if (existingTodo == null)
        {
            return NotFound();
        }

        return Ok(TodoToGetTodoResponse(existingTodo));
    }

    /// <summary>
    /// Create a new TODO
    /// </summary>
    /// <param name="request">The TODO to be created</param>
    /// <returns>A link to the created TODO</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(GetTodoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
    {

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var existingUser = await _dbContext.Users.FindAsync(userId);
        if (existingUser == null)
        {
            return BadRequest();
        }

        var newTodo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId
        };

        try
        {
            _dbContext.Add(newTodo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoById), new { id = newTodo.Id }, TodoToGetTodoResponse(newTodo));
        }
        catch
        {
            return StatusCode(500, "There was an error while creating the TODO");
        }
    }

    /// <summary>
    /// Update a TODO by its ID
    /// </summary>
    /// <param name="id">The ID of the TODO to update</param>
    /// <param name="request">The TODO data to update</param>
    /// <returns>The updated TODO</returns>
    [Authorize]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(GetTodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTodo([FromRoute] long id, [FromBody] UpdateTodoRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var existingUser = await _dbContext.Users.FindAsync(userId);
        if (existingUser == null)
        {
            return BadRequest();
        }

        var existingTodo = await _dbContext.Todos.SingleOrDefaultAsync(t => t.Id == id);
        if (existingTodo == null)
        {
            return NotFound();
        }

        existingTodo.Title = request.Title;
        existingTodo.Description = request.Description;
        try
        {
            _dbContext.Entry(existingTodo).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok(TodoToGetTodoResponse(existingTodo));
        }
        catch
        {
            return StatusCode(500, "There was an error while updating the TODO");
        }
    }

    /// <summary>
    /// Delete a TODO by its ID
    /// </summary>
    /// <param name="id">The ID of the TODO to delete</param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTodo([FromRoute] long id)
    {

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var existingUser = await _dbContext.Users.FindAsync(userId);
        if (existingUser == null)
        {
            return NotFound();
        }

        var existingTodo = await _dbContext.Todos.SingleOrDefaultAsync(t => t.Id == id);
        if (existingTodo == null)
        {
            return NotFound();
        }

        try
        {
            _dbContext.Todos.Remove(existingTodo);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "There was an error while deleting the TODO");
        }
    }

    private static GetTodoResponse TodoToGetTodoResponse(Todo todo)
    {
        return new GetTodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description
        };
    }
}