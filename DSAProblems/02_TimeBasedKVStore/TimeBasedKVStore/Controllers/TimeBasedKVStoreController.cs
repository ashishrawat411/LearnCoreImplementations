using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeBasedKVStore.Interfaces;

namespace TimeBasedKVStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeBasedKVStoreController : ControllerBase
    {
        private readonly ITimeBasedKVStore timeBasedKVStore;
        public TimeBasedKVStoreController(ITimeBasedKVStore _itimeBasedKVStore)
        {
            timeBasedKVStore = _itimeBasedKVStore;
        }

        [HttpPut("{key}")]
        public IActionResult Put(string key, [FromBody] KeyValuePair<long, string> value)
        {
            var result = timeBasedKVStore.AddOrUpdate(key, value);
            return Ok(result);
        }

        [HttpPost("{key}")]
        public IActionResult Add(string key, [FromBody] string value)
        {
            var result = timeBasedKVStore.Add(key, value);
            return result ? Ok(result) : Conflict();
        }


        [HttpDelete("{key}")]
        public IActionResult Delete(string key)
        {
            var result = timeBasedKVStore.Remove(key);
            return result ? Ok(result) : NotFound();
        }

        [HttpGet("{key}")]
        public ActionResult Get(string key)
        {
            var result = timeBasedKVStore.Get(key);
            return result != null ? Ok(result) : NotFound(key);
        }
    }
}
