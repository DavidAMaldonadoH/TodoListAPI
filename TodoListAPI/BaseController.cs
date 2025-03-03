using Microsoft.AspNetCore.Mvc;

namespace TodoListAPI;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BaseController : Controller
{

}
