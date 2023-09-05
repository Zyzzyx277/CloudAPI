using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace CloudSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // GET: api/Files/GetList/5
        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            return await DataAccess.GetFileList(id);
        }

        // GET: api/Files/Get/5
        /*[HttpGet("{id}")]
        public FileObject Get(string id)
        {
            return DataAccess.GetFile(id);
        }*/

        [HttpPost("{idUser}")]
        public async Task<string> Post([FromBody]string idFile, string idUser)
        {
            return JsonConvert.SerializeObject(await DataAccess.GetFile(idFile, idUser));
        }

        // PUT: api/Files/5/5
        [HttpPut("{id}/{key}")]
        public async Task Put(string id, string key, [FromBody]FileObject file)
        {
            await DataAccess.CreateFile(id, key, file);
        }

        // DELETE: api/Files/5
        [HttpDelete("{idUser}/{key}/{idFile}")]
        public async Task Delete(string idFile, string idUser, string key)
        {
            await DataAccess.DeleteFile(idUser, key, idFile);
        }
    }
}
