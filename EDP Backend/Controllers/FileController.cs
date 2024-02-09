﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NanoidDotNet;

namespace EDP_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        public FileController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file.Length > 1024 * 1024 * 10)
            {
                var message = "Maximum file size is 10MB";
                return BadRequest(new { message });
            }

            var id = Nanoid.Generate(size: 10);
            var filename = id + Path.GetExtension(file.FileName);
            var imagePath = Path.Combine(_environment.ContentRootPath,
            @"wwwroot/uploads", filename);
            // create folder in wwwroot
            if (!Directory.Exists(Path.GetDirectoryName(imagePath)))
            {
				Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
			}
            using var fileStream = new FileStream(imagePath, FileMode.Create);
            file.CopyTo(fileStream);
            return Ok(new { filename });
        }
    }
}
