using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RecognizerController : Controller
    {
        private RecognizerBackend backend = new RecognizerBackend();

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] {
                @"Hello! This is a MNIST image recognition web server!
                Here you can:
                POST an image to /api/recognizer/detect -> detect class of an image
                GET /api/recognizer/stats -> get db statistics
                GET /api/recognizer/drop_table -> clear the recognized image db"
            };

        }

        // GET api/recognizer/detect
        [HttpGet("detect")]
        public string GetDetect()
        {
            return "POST an image here to get its class and number of db requests.";
        }

        // POST api/recognizer/detect
        [HttpPost("detect")]
        public string Post()
        {
            byte[] bytes;
            var ms = new MemoryStream(2048);
            Request.Body.CopyTo(ms);
            bytes = ms.ToArray();
            return backend.DetectImage(bytes);
        }

        // GET api/recognizer/detect
        [HttpGet("stats")]
        public string GetStats()
        {
            return backend.GetStatistics();
        }

        // GET api/recognizer/detect
        [HttpGet("drop_table")]
        public string GetDropTable()
        {
            return backend.DropImagesTable();
        }
    }
}
