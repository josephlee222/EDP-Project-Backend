﻿using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Admin/Activity")]
    public class ActivityAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ActivityAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current activities")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetActivities()
        {
            return Ok(_context.Activities);
        }


        [SwaggerOperation(Summary = "Create a new activity")]
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateActivity([FromBody] CreateActivityRequest request)
        {
            string name = request.Name.Trim();
            string? description = request.Description?.Trim();
            DateTime expiryDate = request.ExpiryDate;
            string? discountType = request.DiscountType?.Trim();
            string? category = request.Category?.Trim();
            bool? ntucExclusive = request.NtucExclusive;
            int? ageLimit = request.AgeLimit;
            string? company = request.Company?.Trim();
            string? location = request.Location?.Trim();
            string? discounttype = request.DiscountType?.Trim();
            float? discountAmount = request.DiscountAmount;
            bool? discounted = request.Discounted;
            string[]? pictures = request.Pictures;

            StringArray? arrayPictures = null;

            if (pictures != null)
            {
                arrayPictures = new StringArray { Items = pictures };
            }


            // Check if name is already registered
            Activity? existingActivity = _context.Activities.FirstOrDefault(activity => activity.Name == name);
            if (existingActivity != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Activity with this name already exists"));
            }



            // Create activity
            Activity activity = new Activity
            {
                Name = name,
                Description = description,
                ExpiryDate = expiryDate,
                DiscountType = discountType,
                Category = category,
                NtucExclusive = ntucExclusive,
                AgeLimit = ageLimit,
                Company = company,
                Location = location,
                DiscountAmount = discountAmount,
                Discounted = discounted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Pictures = arrayPictures,

            };

            _context.Activities.Add(activity);
            _context.SaveChanges();
            return Ok(activity);
        }


        [SwaggerOperation(Summary = "Get a specific activity")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Getactivity(int id)
        {
            Activity? activity = _context.Activities.Find(id);
            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("activity not found"));
            }
            return Ok(activity);
        }

        [SwaggerOperation(Summary = "Update a specific activity")]
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Editactivity(int id, [FromBody] EditActivityRequest request)
        {
            // Get activity
            Activity? activity = _context.Activities.Find(id);

            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("activity not found"));
            }

            // Update activity
            string name = request.Name.Trim();
            string? description = request.Description?.Trim();
            DateTime? expiryDate = request.ExpiryDate;
            string? discountType = request.DiscountType?.Trim();
            string? category = request.Category?.Trim();
            bool? ntucExclusive = request.NtucExclusive;
            int? ageLimit = request.AgeLimit;
            string? company = request.Company?.Trim();
            string? location = request.Location?.Trim();
            float? discountAmount = request.DiscountAmount;
            bool? discounted = request.Discounted;
            string[]? pictures = request.Pictures;

            StringArray? arrayPictures = null;

            if (pictures != null)
            {
                arrayPictures = new StringArray { Items = pictures };
            }


            //activity.Code = code ?? activity.Code;

            activity.Name = name ?? activity.Name;
            activity.Description = description ?? activity.Description;
            activity.ExpiryDate = expiryDate ?? activity.ExpiryDate;
            activity.DiscountType = discountType ?? activity.DiscountType;
            activity.Category = category ?? activity.Category;
            activity.NtucExclusive = ntucExclusive ?? activity.NtucExclusive;
            activity.AgeLimit = ageLimit ?? activity.AgeLimit;
            activity.Company = company ?? activity.Company;
            activity.Location = location ?? activity.Location;
            activity.DiscountAmount = discountAmount ?? activity.DiscountAmount;
            activity.Discounted = discounted ?? activity.Discounted;
            activity.Pictures = arrayPictures ?? activity.Pictures;

            _context.SaveChanges();
            return Ok(activity);
        }



        [SwaggerOperation(Summary = "Delete a specific activity")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Deleteactivity(int id)
        {
            Activity? activity = _context.Activities.Find(id);
            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("Activity not found"));
            }

            // Remove the activity from the context
            _context.Activities.Remove(activity);

            // Delete associated files from wwwroot/uploads folder
            if (activity.Pictures != null && activity.Pictures.Items != null)
            {
                foreach (var filename in activity.Pictures.Items)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", filename);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            return Ok(activity);
        }
    }
}
