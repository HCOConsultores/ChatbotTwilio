using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Twilio.Security;
using Bpi.Services.Whatsapp;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace chatBotTwilio.Services.WhatsApp
{
    public class ReceiveWhatsAppService : IReceiveWhatsAppService
    {
        private readonly RequestValidator _requestValidator;
        private readonly ILogger<ReceiveWhatsAppService> _logger;
        private readonly IConversationManagerService _conversationManagerService;
        private readonly ISendWhatsappService _sendWhatsAppService;
    //    private readonly WhatsAppImageServiceFire _whatsAppImageService;
        private readonly WhatsAppImageServiceAzure _whatsAppImageService;
        private readonly string _twilioPhoneNumber;

        public ReceiveWhatsAppService(
            ILogger<ReceiveWhatsAppService> logger,
            IConversationManagerService conversationManagerService,
            ISendWhatsappService sendWhatsAppService,
            //WhatsAppImageServiceFire whatsAppImageService,
            WhatsAppImageServiceAzure whatsAppImageService,
        IConfiguration configuration)
        {
            _logger = logger;
            _conversationManagerService = conversationManagerService;
            _sendWhatsAppService = sendWhatsAppService;
            _whatsAppImageService = whatsAppImageService;
            _requestValidator = new RequestValidator(configuration["Twilio:AuthToken"]);
            _twilioPhoneNumber = configuration["Twilio:PhoneNumber"];
        }

        public string ValidateWebhook(string mode, string challenge, string verify_token)
        {
            if (verify_token.Equals("holaaa"))
            {
                _logger.LogInformation("Webhook validado correctamente");
                return challenge;
            }
            else
            {
                _logger.LogWarning("Error en la validación del webhook");
                return "Error de validación";
            }
        }

        public bool IsValidTwilioRequest(HttpRequest request)
        {
            var signature = request.Headers["X-Twilio-Signature"].FirstOrDefault();
            var url = $"{request.Scheme}://{request.Host}{request.Path}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var pair in request.Form)
            {
                parameters.Add(pair.Key, pair.Value);
            }

            return _requestValidator.Validate(url, parameters, signature);
        }

        public async Task<string> ReceiveWhatsAppMessage(string from, string body, string numMedia, string mediaUrl, string mediaContentType)
        {
            string cleanPhoneNumber = from.StartsWith("whatsapp:") ? from : $"whatsapp:{from}";
            string messageType = !string.IsNullOrEmpty(numMedia) && int.Parse(numMedia) > 0 ? "Media" : "Text";
            string messageContent = !string.IsNullOrEmpty(mediaUrl) ? mediaUrl : body;
            _logger.LogInformation($"Mensaje recibido - Tipo: {messageType}, Teléfono: {cleanPhoneNumber}, Contenido: {messageContent}");

            // Evitar responder a mensajes del propio número de Twilio (prevenir loops)
            string twilioWhatsAppNumber = $"whatsapp:{_twilioPhoneNumber}";
            if (cleanPhoneNumber.Equals(twilioWhatsAppNumber, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"Mensaje ignorado: viene del propio número de Twilio {cleanPhoneNumber}");
                return "Mensaje ignorado para evitar loop";
            }

            string responseMessage;

            if (messageType == "Media" && !string.IsNullOrEmpty(mediaUrl))
            {
                try
                {
                    var (imageUrl, imageDescription) = await _whatsAppImageService.ProcessImageAsync(mediaUrl, body);

                    _logger.LogInformation($"Imagen procesada - URL: {imageUrl}, Descripción: {imageDescription}");

                    // Procesar el mensaje con la información de la imagen
                    responseMessage = await _conversationManagerService.ProcessMessage(cleanPhoneNumber, body, numMedia, imageUrl, mediaContentType, imageDescription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar la imagen");
                    responseMessage = "Lo siento, hubo un error al procesar la imagen. Por favor, intenta nuevamente.";
                }
            }
            else
            {
                // Procesar mensajes de texto normales
                responseMessage = await _conversationManagerService.ProcessMessage(cleanPhoneNumber, body, numMedia, mediaUrl, mediaContentType);
            }

            // Enviar respuesta usando SendWhatsappService
            await _sendWhatsAppService.SendWhatsAppMessage(cleanPhoneNumber, responseMessage);
            _logger.LogInformation($"Respuesta enviada a {cleanPhoneNumber}: {responseMessage}");

            return responseMessage;
        }
    }

    public interface IReceiveWhatsAppService
    {
        Task<string> ReceiveWhatsAppMessage(string from, string body, string numMedia, string mediaUrl, string mediaContentType);
        string ValidateWebhook(string mode, string challenge, string verify_token);
        bool IsValidTwilioRequest(HttpRequest request);
    }
}