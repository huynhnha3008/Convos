using Vonage.Request;
using Vonage;
using Vonage.Messaging;


namespace Services
{
    public class SmsService
    {
        private readonly string _apiKey = "ff93dc79";
        private readonly string _apiSecret = "upwCJajJgpSJl1aq";
        public  async Task SendSms(string toPhoneNumber, string message)
        {
            var credentials = Credentials.FromApiKeyAndSecret(_apiKey, _apiSecret);
            var vonageClient = new VonageClient(credentials);

            var request = new SendSmsRequest
            {
                To = toPhoneNumber,
                From = "Vonage APIs", 
                Text = message
            };

            
            var response = await vonageClient.SmsClient.SendAnSmsAsync(request);


            if (response.Messages[0].Status != "0")
            {
                throw new Exception($"SMS failed with status: {response.Messages[0].ErrorText}");
            }

            Console.WriteLine("SMS sent successfully.");
        }
        
    }
}
