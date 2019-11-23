using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RecognizerController : Controller
    {
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

        // POST api/<controller>
        [HttpPost("detect")]
        public string Post([FromBody]string value)
        {

            return "Work in progress";
        }

        // GET api/recognizer/detect
        [HttpGet("stats")]
        public string GetStats()
        {
            return "Returns JSON string with db statistics.";
        }

        // GET api/recognizer/detect
        [HttpGet("drop_table")]
        public string GetDropTable()
        {
            return "Returns some success code probably.";
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
