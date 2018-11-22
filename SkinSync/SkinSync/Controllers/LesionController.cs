using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using SkinSync.Helpers;
using SkinSync.Models;

namespace SkinSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LesionController : ControllerBase
    {
        private readonly SkinSyncContext _context;
        private IConfiguration _configuration;

        public LesionController(SkinSyncContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Lesion
        [HttpGet]
        public IEnumerable<LesionItem> GetLesionItem()
        {
            return _context.LesionItem;
        }

        // GET: api/Lesion/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLesionItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesionItem = await _context.LesionItem.FindAsync(id);

            if (lesionItem == null)
            {
                return NotFound();
            }

            return Ok(lesionItem);
        }

        // PUT: api/Lesion/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLesionItem([FromRoute] int id, [FromBody] LesionItem lesionItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != lesionItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(lesionItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LesionItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Lesion
        [HttpPost]
        public async Task<IActionResult> PostLesionItem([FromBody] LesionItem lesionItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.LesionItem.Add(lesionItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLesionItem", new { id = lesionItem.Id }, lesionItem);
        }

        // DELETE: api/Lesion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesionItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesionItem = await _context.LesionItem.FindAsync(id);
            if (lesionItem == null)
            {
                return NotFound();
            }

            _context.LesionItem.Remove(lesionItem);
            await _context.SaveChangesAsync();

            return Ok(lesionItem);
        }

        private bool LesionItemExists(int id)
        {
            return _context.LesionItem.Any(e => e.Id == id);
        }

        // GET: api/Lesion/Location
        [HttpGet]
        [Route("lesion")]
        public async Task<List<LesionItem>> GetLocationItem([FromQuery] string location)
        {
            var lesions = from l in _context.LesionItem
                        select l; //get all the lesions


            if (!String.IsNullOrEmpty(location)) //make sure user gave a location tag to search for
            {
                lesions = lesions.Where(s => s.Location.ToLower().Equals(location.ToLower())); // find the entries with the location search tag and reassign
            }

            var returned = await lesions.ToListAsync(); //return the lesions with the specified location tag

            return returned;
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]LesionImageItem lesion)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = lesion.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(lesion.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    LesionItem lesionItem = new LesionItem();
                    lesionItem.Location = lesion.Location;
                    lesionItem.Diameter = Convert.ToInt32(lesion.Diameter); // Convert user string input to int

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    lesionItem.Height = image.Height.ToString();
                    lesionItem.Width = image.Width.ToString();
                    lesionItem.Url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;
                    lesionItem.Uploaded = DateTime.Now.ToString();

                    _context.LesionItem.Add(lesionItem);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {lesion.Location} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }

    }

}