
using Microsoft.AspNetCore.Mvc;
using chatBotTwilio.Services.WhatsApp;

namespace chatBotTwilio.Controllers.Whatsapp
{

    public class ReceiveController : ControllerBase
    {
        private readonly IReceiveWhatsAppService _receiveWhatsAppService;
        private readonly ILogger<ReceiveController> _logger;

        public ReceiveController(IReceiveWhatsAppService receiveWhatsAppService, ILogger<ReceiveController> logger)
        {
            _receiveWhatsAppService = receiveWhatsAppService;
            _logger = logger;
        }

        [HttpGet]
        [Route("webhook")]
        public IActionResult Webhook(

          [FromQuery(Name = "hub.mode")] string mode,
          [FromQuery(Name = "hub.challenge")] string challenge,
          [FromQuery(Name = "hub.verify_token")] string verify_token)
        {
            _logger.LogInformation("Webhook GET llamado");
            var result = _receiveWhatsAppService.ValidateWebhook(mode, challenge, verify_token);
            return Ok(result);
        }

        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> Post(
            [FromForm] string From,
            [FromForm] string Body,
            [FromForm] string NumMedia,
            [FromForm] string MediaUrl0,
            [FromForm] string MediaContentType0)
        {
            _logger.LogInformation($"Datos recibidos: {Body}");
            // Validar la solicitud de Twilio
            if (!_receiveWhatsAppService.IsValidTwilioRequest(Request))
            {
                _logger.LogWarning("Solicitud no válida recibida");
                return Unauthorized();
            }

            var response = await _receiveWhatsAppService.ReceiveWhatsAppMessage(From, Body, NumMedia, MediaUrl0, MediaContentType0);
            return Ok(new { message = response });
        }

    }
}