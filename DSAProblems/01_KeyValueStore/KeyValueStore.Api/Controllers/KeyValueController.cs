using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KeyValueStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyValueController : ControllerBase
    {
        private readonly IKeyValueStore _keyValueStore;

        public KeyValueController(IKeyValueStore keyValueStore)
        {
            _keyValueStore = keyValueStore;
        }

        [HttpGet("{key}")]
        public ActionResult<string> Get(string key)
        {
            string? value =  _keyValueStore.Get(key);
            return Ok(value);
        }
        
        [HttpPut("{key}")]
        public IActionResult Put(string key, [FromBody] string value)
        {
            var result = _keyValueStore.Set(key, value);
            return Ok(result);
        }

        [HttpDelete("{key}")]
        public IActionResult Delete(string key)
        {
            var result = _keyValueStore.Delete(key);
            return Ok(result);
        }

        [HttpGet(Name = "ListAllKeys")]
        public ActionResult List()
        {
            return Ok(_keyValueStore.List());
        }
    }
}
