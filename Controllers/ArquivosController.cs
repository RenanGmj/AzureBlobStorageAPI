using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobStorageAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArquivosController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _containername;

        public ArquivosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("BlogConnectionString");
            _containername = configuration.GetValue<string>("BlogContainerName");
        }
        
        [HttpPost("Upload")]
        public IActionResult UploadArquivo(IFormFile arquivo)
        {
            //BLOB = binary large object
            BlobContainerClient container = new(_connectionString, _containername);
            BlobClient blob = container.GetBlobClient(arquivo.FileName);

            using var data = arquivo.OpenReadStream();
            blob.Upload(data, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders {ContentType = arquivo.ContentType}
            });

            return Ok(blob.Uri.ToString());
        } 
        
        [HttpGet("Dowload/{nome}")]
        public IActionResult DowloadArquivo(string nome)
        {
            BlobContainerClient container = new(_connectionString, _containername);
            BlobClient blob = container.GetBlobClient(nome);

            if (!blob.Exists())
                return BadRequest();

            var retorno = blob.DownloadContent();
            return File(retorno.Value.Content.ToArray(), retorno.Value.Details.ContentType, blob.Name);
        }
        
        [HttpDelete("Apagar/{nome}")]
        public IActionResult DeletarArquivo(string nome)
        {
            BlobContainerClient container = new(_connectionString, _containername);
            BlobClient blob = container.GetBlobClient(nome);

            blob.DeleteIfExists();
            return NoContent();
        }

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            List<BlobDto> blobsDto = new List<BlobDto>();
            BlobContainerClient container = new(_connectionString, _containername);

            foreach(var blob in container.GetBlobs())
            {
                blobsDto.Add(new BlobDto
                {
                    Nome = blob.Name,
                    Tipo = blob.Properties.ContentType,
                    Uri = container.Uri.AbsoluteUri + "/" + blob.Name

                });
                
            }

            return Ok(blobsDto);
        }

    }
}