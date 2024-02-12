﻿using System.Security.Claims;
using EDP_Backend.Hubs;
using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
		private readonly IHubContext<GroupsHub> _hubContext;

		public GroupController(MyDbContext context, IConfiguration configuration, IHubContext<GroupsHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
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
        public IActionResult CreateGroup([FromBody] CreateGroup request)
        {
            string name = request.Name.Trim();
            string description = request.Description.Trim();

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
        public IActionResult Editgroup(int id, [FromBody] EditGroup request)
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



		[SwaggerOperation(Summary = "Send a message into a specific group")]
		[HttpPost("Message"), Authorize]
        public IActionResult SendMessage([FromBody] SendMessageRequest request)
        {
			// Get user id
			int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Get group
            Group? group = _context.Groups.Find(request.GroupId);
            if (group == null)
            {
				return NotFound(Helper.Helper.GenerateError("Group not found"));
			}

            // Get user
            User? user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(Helper.Helper.GenerateError("User not found"));
            }

            // Create message
            GroupMessage message = new()
			{
				Group = group,
                User = user,
                Message = request.Message,
			};

            _context.GroupMessages.Add(message);
			_context.SaveChanges();
			_hubContext.Clients.Groups(group.Id.ToString()).SendAsync("message", message);
			return Ok(message);
		}

		[SwaggerOperation(Summary = "Retrieve all messages from the group")]
		[HttpGet("Message/{id}"), Authorize]
        public IActionResult GetMessages(int id)
        {
			var messages = _context.GroupMessages.Include(m => m.Group).Include(m => m.User).Where(m => m.Group.Id == id).OrderBy(m => m.CreatedAt);
			return Ok(messages);
		}
	}
}
