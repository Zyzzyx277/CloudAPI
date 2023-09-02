using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // GET: api/Files/GetList/5
        [HttpGet("{id}")]
        public IEnumerable<byte[]> Get(string id)
        {
            return DataAccess.GetFileList(id);
        }

        /*// GET: api/Files/Get/5
        [HttpGet("{id}")]
        public FileObject Get(string id)
        {
            return DataAccess.GetFile(id);
        }*/

        [HttpPost("{id}")]
        public void Post(string id)
        {
        }

        // PUT: api/Files/5
        [HttpPut("{id}/{key}")]
        public void Put(string id, string key, [FromBody]FileObject file)
        {
            DataAccess.CreateFile(id, key, file);
        }

        // DELETE: api/Files/5
        [HttpDelete("{id}")]
        public void Delete(string idFile, string idUser, [FromBody]string key)
        {
            DataAccess.DeleteFile(idUser, key, idFile);
        }
    }
}
