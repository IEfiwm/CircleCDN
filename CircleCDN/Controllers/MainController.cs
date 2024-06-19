using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CircleCDN.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        public MainController(ILogger<MainController> logger, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [Route("/{filename}")]
        [HttpGet]
        [ResponseCache(Duration = 86400, NoStore = false, Location = ResponseCacheLocation.Any)]
        public IActionResult Get(string filename)
        {
            try
            {
                string root = Environment.GetEnvironmentVariable("AppSettings_Root");

                if (string.IsNullOrEmpty(root))
                    root = _configuration["AppSettings:Root"];

                var pathToFile = Path.Combine(root, filename);

                if (!_memoryCache.TryGetValue(filename, out FileStream fileStream))
                {
                    if (!System.IO.File.Exists(pathToFile))
                    {
                        return NotFound();
                    }

                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };

                    fileStream = System.IO.File.Open(pathToFile, FileMode.Open, FileAccess.Read);
                    _memoryCache.Set(filename, fileStream, cacheExpiryOptions);
                }

                var ext = Path.GetExtension(pathToFile).TrimStart('.');

                return File(fileStream, $"image/{ext}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error serving file. Falling back to default photo.");

                string defaultPhoto = Environment.GetEnvironmentVariable("AppSettings:DefaultPhoto");

                if (string.IsNullOrEmpty(defaultPhoto))
                    defaultPhoto = _configuration["AppSettings:DefaultPhoto"];

                var file = System.IO.File.OpenRead(defaultPhoto);
                var ext = Path.GetExtension(defaultPhoto).TrimStart('.');

                return File(file, $"image/{ext}");
            }
        }

        [HttpPost("/Upload")]
        public IActionResult Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file");
                }
                string root = Environment.GetEnvironmentVariable("AppSettings_Root");

                if (string.IsNullOrEmpty(root))
                    root = _configuration["AppSettings:Root"];

                var filename = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var pathToFile = Path.Combine(root, filename);

                using (var stream = new FileStream(pathToFile, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                OptimizeImage(pathToFile);

                // Additional logic if needed, e.g., saving file information to a database

                return Ok(filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading file");
                return StatusCode(500, "Internal server error");
            }
        }

        private void OptimizeImage(string path)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(path).ToLower();

            if (validExtensions.Contains(ext))
            {
                using (var image = new MagickImage(path))
                {
                    image.Format = Enum.TryParse(ext.TrimStart('.'), true, out MagickFormat format)
                        ? format
                        : MagickFormat.Jpg;

                    // Example: Resize image to a maximum of 800x800 pixels
                    image.Resize(800, 800);

                    image.Quality = 65;

                    // Additional optimization steps if needed

                    image.Write(path);
                }
            }
        }
    }
}