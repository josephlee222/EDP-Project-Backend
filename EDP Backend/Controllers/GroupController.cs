using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;



namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("/Group")]
    public class GroupController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public GroupController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current groups")]
        [HttpGet(), Authorize]
        public IActionResult GetGroups()
        {
            return Ok(_context.Groups);
        }


        [SwaggerOperation(Summary = "Get a specific group")]
        [HttpGet("{id}"), Authorize]
        public IActionResult GetGroup(int id)
        {
            Group? group = _context.Groups.Find(id);
            if (group == null)
            {
                return NotFound(Helper.Helper.GenerateError("Group not found"));
            }
            return Ok(group);
        }


        [SwaggerOperation(Summary = "Create a new group")]
        [HttpPost(), Authorize]
        public IActionResult CreateGroup([FromBody] CreateGroupRequest request)
        {
            string name = request.Name.Trim();
            string description = request.Description.Trim();

            // Check if name is already registered (TO BE REMOVED)
            Group? existingGroup = _context.Groups.FirstOrDefault(group => group.Name == name);
            if (existingGroup != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Group with this name already exists"));
            }

            // Create group
            Group group = new Group
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            _context.SaveChanges();
            return Ok(group);
        }


        [SwaggerOperation(Summary = "Update a specific group")]
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Editgroup(int id, [FromBody] EditGroupRequest request)
        {
            // Get group
            Group? group = _context.Groups.Find(id);

            // Check if group exists
            if (group == null)
            {
                return NotFound(Helper.Helper.GenerateError("group not found"));
            }

            // Update group
            string name = request.Name.Trim();
            string? description = request.Description?.Trim();

            //group.Code = code ?? group.Code;

            group.Name = name ?? group.Name;
            group.Description = description ?? group.Description;

            _context.SaveChanges();
            return Ok(group);
        }

        [SwaggerOperation(Summary = "Delete a specific group")]
        [HttpDelete("{id}"), Authorize]
        public IActionResult Deletegroup(int id)
        {
            Group? group = _context.Groups.Find(id);
            if (group == null)
            {
                return NotFound(Helper.Helper.GenerateError("group not found"));
            }
            _context.Groups.Remove(group);
            _context.SaveChanges();
            return Ok(group);
        }
    }
}
