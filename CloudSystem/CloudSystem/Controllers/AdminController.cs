using CloudSystem.Model;
using Microsoft.AspNetCore.Mvc;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // DELETE: api/Admin/5
        [HttpDelete]
        public IActionResult Delete()
        {
            string? key = Request.Headers["key"];
            if (key is null) return BadRequest("Key not set");
            string status = Permissions.DeleteAll(key);
            if (string.IsNullOrEmpty(status)) return Ok("All Data has been deleted");
            return BadRequest(status);
        }
    }
}
