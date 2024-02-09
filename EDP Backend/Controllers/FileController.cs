using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NanoidDotNet;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EDP_Backend.Controllers
{
    [Route("/File")]
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


        [HttpPost("multiUpload")]
        public async Task<IActionResult> multiUpload(List<IFormFile> files)
        {

            // Check if any files were uploaded
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files were uploaded."+files);
            }
            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                // Check file size
                if (file.Length > 1024 * 1024 * 10)
                {
                    return BadRequest("Maximum file size is 10MB");
                }

                // Generate unique filename
                var id = Nanoid.Generate(size: 10);
                var filename = id + Path.GetExtension(file.FileName);
                var imagePath = Path.Combine(_environment.ContentRootPath, @"wwwroot/uploads", filename);

                // Save file to disk
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Add filename to the list of uploaded files
                uploadedFiles.Add(filename);
            }

            return Ok(new { uploadedFiles });
        }
    }
}
