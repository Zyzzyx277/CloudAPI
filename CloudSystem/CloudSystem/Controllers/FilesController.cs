using CloudSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

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

        // GET: api/Files/Get/5/5
        [HttpGet("{userId}/{fileId}")]
        public IActionResult Get(string userId, string fileId)
        {
            try
            {
                StorageAccess.sem.WaitOne();
                string filePath = $"/data/content/{userId}/{fileId}.json";
                if (!System.IO.File.Exists(filePath)) return BadRequest("File Not Found");
                return File(System.IO.File.OpenRead(filePath), "application/octet-stream", Path.GetFileName(filePath));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                return BadRequest("IO Error");
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
                return BadRequest("IO Error");
            }
            finally
            {
                StorageAccess.sem.Release();
            }
        }

        // PUT: api/Files/5/5
        [HttpPut("{userId}/{fileId}")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        public async Task<IActionResult> Put(string userId, string fileId)
        {
            string? path = Request.Headers["path"];
            string? key = Request.Headers["key"];
            string? compress = Request.Headers["compress"];
            if (key is null || path is null || compress is null) return BadRequest("Property not set");
            bool compressBool;
            if (!bool.TryParse(compress, out compressBool)) return BadRequest("Wrong Format of Argument");
            
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                return BadRequest("Not a multipart request");

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            var reader = new MultipartReader(boundary, Request.Body);

            // note: this is for a single file, you could also process multiple files
            var section = await reader.ReadNextSectionAsync();

            if (section == null)
                return BadRequest("No sections in multipart defined");

            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                return BadRequest("No content disposition in multipart defined");

            var fileName = contentDisposition.FileNameStar.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = contentDisposition.FileName.ToString();
            }

            if (string.IsNullOrEmpty(fileName))
                return BadRequest("No filename defined.");

            await using var fileStream = section.Body;
            string status = await DataAccess.CreateFile(userId, key, fileId, path, compressBool, fileStream);
            if (string.IsNullOrEmpty(status)) return Ok();
            return BadRequest(status);
        }

        // DELETE: api/Files/5
        [HttpDelete("{idUser}/{idFile}")]
        public async Task<IActionResult> Delete(string idFile, string idUser)
        {
            string? key = Request.Headers["key"];
            if (key is null) return BadRequest("Key not set");
            
            string status = await DataAccess.DeleteFile(idUser, key, idFile);
            if (string.IsNullOrEmpty(status)) return Ok();
            return BadRequest(status);
        }
    }
}
