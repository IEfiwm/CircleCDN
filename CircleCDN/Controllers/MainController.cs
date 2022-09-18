using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CircleCDN.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;

        private readonly IConfiguration _configuration;

        private readonly IMemoryCache _memoryCache;

        public MainController(ILogger<MainController> logger, IConfiguration configurationRoot, IMemoryCache memoryCache)
        {
            _logger = logger;

            _configuration = configurationRoot;

            _memoryCache = memoryCache;
        }

        [Route("/{filename}")]
        [HttpGet]
        [ResponseCache(Duration = 86400, NoStore = false, Location = ResponseCacheLocation.Any)]
        public IActionResult Get(string filename)
        {
            try
            {
                var root = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["Root"];

                var pathToFile = $@"{root}\{filename}";

                if (!_memoryCache.TryGetValue(filename, out FileStream fileStream))
                {
                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddSeconds(50),
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromSeconds(20)
                    };

                    if (!System.IO.File.Exists(pathToFile))
                    {
                        return NotFound();
                    }

                    fileStream = System.IO.File.Open(pathToFile, System.IO.FileMode.Append, System.IO.FileAccess.Write);

                    _memoryCache.Set(filename, fileStream, cacheExpiryOptions);
                }

                var meta = new FileInfo(pathToFile);

                var ext = Regex.Replace(meta.Extension, @"(\.)", " ").Trim();

                return File(fileStream, $"image/{ext}");
            }
            catch (Exception e)
            {
                var defualtPhoto = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["DefualtPhoto"];

                var file = System.IO.File.OpenRead(defualtPhoto);

                var meta = new FileInfo(defualtPhoto);

                var ext = Regex.Replace(meta.Extension, @"(\.)", " ").Trim();

                return File(file, $"image/{ext}");
            }
        }
    }
}