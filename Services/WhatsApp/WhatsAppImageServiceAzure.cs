using Azure.Storage.Blobs;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Bpi.Services.Whatsapp
{
    public class WhatsAppImageServiceAzure
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppImageServiceAzure> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _accountSid;
        private readonly string _authToken;

        public WhatsAppImageServiceAzure(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WhatsAppImageServiceAzure> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration;
            _logger = logger;

            var connectionString = _configuration["AzureStorageConfig:ConnectionString"];
            _containerName = _configuration["AzureStorageConfig:ContainerName"];
            _accountSid = _configuration["Twilio:AccountSid"];
            _authToken = _configuration["Twilio:AuthToken"];

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(_containerName))
            {
                throw new ArgumentNullException("Azure Storage configuration is missing or incomplete.");
            }

            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken))
            {
                throw new ArgumentNullException("Twilio configuration is missing or incomplete.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<(string url, string description)> ProcessImageAsync(string mediaUrl, string description)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Iniciando ProcessImageAsync");

                if (string.IsNullOrEmpty(mediaUrl))
                {
                    _logger.LogWarning("URL de la imagen no proporcionada.");
                    return ("URL_NO_DISPONIBLE", "No se proporcionó URL de la imagen");
                }
                _logger.LogInformation("URL de imagen recibida: {MediaUrl}", mediaUrl);

                byte[] imageData = await DownloadImageAsync(mediaUrl);
                if (imageData == null || imageData.Length == 0)
                {
                    _logger.LogWarning("No se pudo descargar la imagen. URL: {MediaUrl}", mediaUrl);
                    return (mediaUrl, "No se pudo descargar la imagen");
                }

                string blobUrl = await UploadToBlobStorageAsync(imageData, description);

                if (string.IsNullOrEmpty(blobUrl))
                {
                    _logger.LogWarning("No se pudo subir la imagen a Azure Blob Storage.");
                    return (mediaUrl, "Imagen no subida a Azure");
                }

                _logger.LogInformation("Proceso de imagen completado en {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return (blobUrl, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image. Elapsed time: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return ("ERROR", "Se produjo un error al procesar la imagen");
            }
        }

        private async Task<byte[]> DownloadImageAsync(string url)
        {
            using var client = _httpClientFactory.CreateClient();
            try
            {
                _logger.LogInformation("Iniciando descarga de imagen desde URL: {Url}", url);
                
                // Agregar autenticación HTTP Basic para URLs de Twilio
                if (url.Contains("api.twilio.com"))
                {
                    var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
                    _logger.LogInformation("Agregada autenticación HTTP Basic para URL de Twilio");
                }
                
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();

                _logger.LogInformation("Imagen descargada exitosamente. Tamaño: {Size} bytes", imageData.Length);
                return imageData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al descargar la imagen desde {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al descargar la imagen desde {Url}", url);
                return null;
            }
        }


        private async Task<string> UploadToBlobStorageAsync(byte[] imageData, string description)
        {
            try
            {
                _logger.LogInformation("Iniciando carga a Azure Blob Storage. Tamaño de imagen: {Size} bytes", imageData.Length);
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();
                string fileName = $"whatsapp_image_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var memoryStream = new MemoryStream(imageData))
                {
                    await blobClient.UploadAsync(memoryStream, true);
                }

                // Sanitize the description to ensure it only contains ASCII characters
                string sanitizedDescription = SanitizeString(description);

                // Agregar la descripción sanitizada como metadatos del blob
                        var metadata = new Dictionary<string, string>
                {
                    { "Description", sanitizedDescription }
                };

                await blobClient.SetMetadataAsync(metadata);

                _logger.LogInformation("Imagen cargada exitosamente a Azure Blob Storage. URL: {BlobUrl}", blobClient.Uri);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar a Azure Blob Storage");
                return null;
            }
        }

        private string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove any non-ASCII characters
            return new string(input.Where(c => c < 128).ToArray());
        }

    }
}