using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;
using Twilio;

namespace chatBotTwilio.Services.WhatsApp
{
    public class SendWhatsappService : ISendWhatsappService
    {

        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioPhoneNumber;
        private readonly ILogger<ReceiveWhatsAppService> _logger;

        public SendWhatsappService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _twilioPhoneNumber = configuration["Twilio:PhoneNumber"];

            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken))
            {
                throw new InvalidOperationException("Twilio AccountSid or AuthToken is not configured.");
            }

            TwilioClient.Init(_accountSid, _authToken);
        }


        public async Task<MessagingResponse> SendWhatsAppMessage(string to, string message)
        {
            // Asegúrate de que 'to' tenga el prefijo "whatsapp:"
            string formattedTo = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";

            var messageOptions = new CreateMessageOptions(new PhoneNumber(formattedTo))
            {
                From = new PhoneNumber($"whatsapp:{_twilioPhoneNumber}"),
                Body = message
            };

            await MessageResource.CreateAsync(messageOptions);
            var response = new MessagingResponse();
            response.Message(message);
            return response;
        }

    }


    public interface ISendWhatsappService
    {
        Task<MessagingResponse> SendWhatsAppMessage(string to, string message);
    }

}