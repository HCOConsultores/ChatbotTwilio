using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using chatBotTwilio.Models;

namespace chatBotTwilio.Controllers.BPI
{

    [ApiController]
    [Route("api/[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IOptionsMonitor<AzureStorageConfig> _azureConfig;

        public BlobController(IOptionsMonitor<AzureStorageConfig> azureConfig)
        {
            _azureConfig = azureConfig;
        }

        [HttpPost("GetBlobSasUrl")]
        public IActionResult GetBlobSasUrl([FromBody] BlobRequest request)
        {
            try
            {
                var uri = new Uri(request.Url);

                // Extraer accountName, containerName y blobName
                var accountName = uri.Host.Split('.')[0];

                var configName = accountName.Equals("angularbpi", StringComparison.OrdinalIgnoreCase)
                    ? "Legacy"
                    : "Primary";
                var config = _azureConfig.Get(configName);
                var accountKey = config.AccountKey;
                var pathSegments = uri.AbsolutePath.Split('/').Skip(1).ToArray();
                var containerName = pathSegments[0];
                var blobName = string.Join('/', pathSegments.Skip(1));

                // Crea un cliente de BlobServiceClient
                var blobServiceClient = new BlobServiceClient(new Uri($"https://{accountName}.blob.core.windows.net"), new StorageSharedKeyCredential(accountName, accountKey));
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                // Configura los permisos SAS
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b", // Para blobs
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // La URL SAS expirará en 1 hora
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Genera la URL SAS
                var sasQueryParameters = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey));
                var sasUrl = $"{blobClient.Uri}?{sasQueryParameters}";

                return Ok(new { url = sasUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class BlobRequest
    {
        public string Url { get; set; }
    }
}

