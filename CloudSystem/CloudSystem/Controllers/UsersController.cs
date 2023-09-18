using CloudSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public async Task<string> Get()
        {
            var users = await DataAccess.GetUser();
            return JsonConvert.SerializeObject(users.Select(p => new UserClient(p.Id, p.PublicKey)));
        }

        private class UserClient
        {
            public string Id { get; set; }
            public string PublicKey { get; set; }

            public UserClient(string id, string publicKey)
            {
                Id = id;
                PublicKey = publicKey;
            }
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
            var users = await DataAccess.GetUser();
            var existingIds = users.Select(p => p.Id).ToArray();
            string id;
            do
            {
                id = Guid.NewGuid().ToString();
            } while (existingIds.Contains(id));
            await DataAccess.CreateUser(id, publicKey);
            return id;
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string? key = Request.Headers["key"];
            if (key is null) return BadRequest("Key not set");
            string status = await DataAccess.DeleteUser(id, key);
            if (string.IsNullOrEmpty(status)) return Ok();
            return BadRequest(status);
        }
    }
}
