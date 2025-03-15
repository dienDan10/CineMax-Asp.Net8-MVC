using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using System.Diagnostics;

using Task = System.Threading.Tasks.Task;

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string receiverEmail, string subject, string htmlMessage)
        {
            var apiInstance = new TransactionalEmailsApi();
            // create a sender
            string SenderName = _configuration.GetValue<string>("BrevoApi:SenderName") ?? "CineMax";
            string SenderEmail = _configuration.GetValue<string>("BrevoApi:SenderEmail") ?? "contact@cinemax.com";
            SendSmtpEmailSender sender = new SendSmtpEmailSender(SenderName, SenderEmail);

            // create a receiver
            string ToName = "Customer";
            SendSmtpEmailTo receiver1 = new SendSmtpEmailTo(receiverEmail, ToName);
            List<SendSmtpEmailTo> To = [receiver1];

            string TextContent = null;

            try
            {
                var sendSmtpEmail = new SendSmtpEmail(sender, To, null, null, htmlMessage, TextContent, subject);
                return apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }

            return Task.CompletedTask;
        }


    }
}
