using CloudSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var users = await DataAccess.GetUser();
            return users.Select(JsonConvert.SerializeObject);
        }

        // GET: api/Users/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<string> Get(string id)
        {
            return JsonConvert.SerializeObject(await DataAccess.GetUser(id));
        }

        // POST: api/Users/5
        [HttpPost("{id}")]
        public async Task<string> Post(string id)
        {
            return await DataAccess.CreateSessionKey(id);
        }

        // PUT: api/Users
        [HttpPut]
        public async Task<string> Put([FromBody] string publicKey)
        {
            string id = Guid.NewGuid().ToString();
            await DataAccess.CreateUser(id, publicKey);
            return id;
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}/{key}")]
        public async Task Delete(string id, string key)
        {
            await DataAccess.DeleteUser(id, key);
        }
    }
}
