using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todoapp_backend.Data;

namespace todoapp_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        [HttpGet]
        public async Task<ActionResult> GetTodos([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var userId = GetUserId();
            var query = _context.Todos.Where(t => t.UserId == userId);
            var totalCount = await query.CountAsync();

            var todos = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TodoDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    IsCompleted = t.IsCompleted,
                    CompletedAt = t.CompletedAt,
                    UserId = t.UserId
                })
                .ToListAsync();

            return Ok(new
            {
                items = todos,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoDto>> GetTodoById(int id)
        {
            var userId = GetUserId();
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound(new { message = "Todo not found or not authorized." });

            var dto = new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                CreatedAt = todo.CreatedAt,
                IsCompleted = todo.IsCompleted,
                CompletedAt = todo.CompletedAt,
                UserId = todo.UserId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<TodoDto>> CreateTodo([FromBody] TodoCreateDto createDto)
        {
            var userId = GetUserId();

            var todo = new Todo
            {
                Title = createDto.Title.Trim(),
                Description = createDto.Description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
                CompletedAt = null,
                UserId = userId
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            var responseDto = new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                CreatedAt = todo.CreatedAt,
                IsCompleted = todo.IsCompleted,
                CompletedAt = todo.CompletedAt,
                UserId = todo.UserId
            };

            return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, responseDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto updateDto)
        {
            var userId = GetUserId();
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound(new { message = "Todo not found or not authorized." });

            todo.Title = updateDto.Title.Trim();
            todo.Description = updateDto.Description?.Trim() ?? string.Empty;

            // Handle transition of completion state to set completed timestamp
            if (updateDto.IsCompleted && !todo.IsCompleted)
            {
                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.UtcNow;
            }
            else if (!updateDto.IsCompleted && todo.IsCompleted)
            {
                todo.IsCompleted = false;
                todo.CompletedAt = null;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteTodo(int id)
        {
            var userId = GetUserId();
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound(new { message = "Todo not found or not authorized." });

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todo deleted successfully." });
        }
    }
}
