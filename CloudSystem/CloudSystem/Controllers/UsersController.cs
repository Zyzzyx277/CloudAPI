using CloudSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return DataAccess.GetUser();
        }

        // GET: api/Users/5
        [HttpGet("{id}", Name = "Get")]
        public User Get(string id)
        {
            return DataAccess.GetUser(id);
        }

        // POST: api/Users/5
        [HttpPost("{id}")]
        public string Post(string id)
        {
            return DataAccess.CreateSessionKey(id);
        }

        // PUT: api/Users
        [HttpPut]
        public string Put([FromBody] string publicKey)
        {
            string id = Guid.NewGuid().ToString();
            DataAccess.CreateUser(id, publicKey);
            return id;
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}/{key}")]
        public void Delete(string id, string key)
        {
            DataAccess.DeleteUser(id, key);
        }
    }
}
