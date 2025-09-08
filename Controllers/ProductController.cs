using Microsoft.AspNetCore.Mvc;
using ProductWorkflow.API.Models;
using ProductWorkflow.API.Services;

namespace ProductWorkflow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly JobService _jobService;

        public ProductController(ProductService productService, JobService jobService)
        {
            _productService = productService;
            _jobService = jobService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            return product == null ? NotFound() : Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            var created = await _productService.Create(product);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if(id!=product.Id) return BadRequest("Product id didn't match");

            await _productService.Update(product);

            //return NoContent();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if(product == null) return NotFound();

            await _productService.Delete(id);

            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Csv file basic validation
            if (file == null || file.Length == 0)
                return BadRequest("No file found");

            const long maxFileSize = 2 * 1024 * 1024; // 2MB File size limit
            if (file.Length > maxFileSize) 
                return BadRequest("File size exceed the limit");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".csv")
                return BadRequest("Only .csv files are allowed");

            // Validate Header
            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync();
            var expectedHeader = new[] { "Name", "Description", "Price", "Category" };
            if(header==null || !expectedHeader.All(h=> header.Contains(h)))
                return BadRequest("Invalid headers");

            // Make sure file has at least 10 rows of data
            int rowCount = 0;
            while (await reader.ReadLineAsync() is not null)
            {
                rowCount++;
                if (rowCount >= 10)
                    break; 
            }

            if (rowCount < 10)
                return BadRequest("File must contain at least 10 rows of data");

            // Start saving file to disk
            // Get or create folder
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if(!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // FilePath with unique file name
            var filePath= Path.Combine(uploadFolder, $"{Guid.NewGuid()}{extension}");

            // Creating new file stream and saving to disk
            using var saveStream = new FileStream(filePath, FileMode.Create);
            using var uploadStream = file.OpenReadStream();
            await uploadStream.CopyToAsync(saveStream);

            // Create the Job
            var job = new ProcessingJob
            {
                Id = Guid.NewGuid(),
                FilePath = filePath,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _jobService.AddJob(job);

            // Return JobId and status URL
            var statusUrl = Url.Action("GetStatus", new { jobId = job.Id });
            return Accepted(new { JobId = job.Id, StatusUrl = statusUrl });
        }

        [HttpGet("getstatus/{jobId}")]
        public async Task<IActionResult> GetStatus(string jobId)
        {
            if (!Guid.TryParse(jobId, out Guid jobGuid))
                return BadRequest("Invalid JobId");

            var job = await _jobService.GetJobAsync(jobGuid);

            return job == null ? NotFound() : Ok(job);
        }



    }
}
