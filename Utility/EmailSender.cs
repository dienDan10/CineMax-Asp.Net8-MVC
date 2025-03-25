using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
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

        public async Task SendEmailWithAttachmentAsync(string receiverEmail, string subject, string htmlMessage, byte[] attachmentBytes, string attachmentName)
        {
            var apiInstance = new TransactionalEmailsApi();

            // create a sender
            string SenderName = _configuration.GetValue<string>("BrevoApi:SenderName") ?? "CineMax";
            string SenderEmail = _configuration.GetValue<string>("BrevoApi:SenderEmail") ?? "contact@cinemax.com";
            SendSmtpEmailSender sender = new SendSmtpEmailSender(SenderName, SenderEmail);

            var receivers = new List<SendSmtpEmailTo>
            {
                new SendSmtpEmailTo(receiverEmail, "Customer")
            };

            var attachments = new List<SendSmtpEmailAttachment>();
            if (attachmentBytes != null && attachmentBytes.Length > 0)
            {
                attachments.Add(new SendSmtpEmailAttachment(
                    name: attachmentName,
                    content: attachmentBytes
                ));
            }

            var email = new SendSmtpEmail(
                sender: sender,
                to: receivers,
                null, //bcc
                null, // cc
                htmlContent: htmlMessage,
                null, // text content
                subject: subject,
                null, // reply to
                attachment: attachments.Count > 0 ? attachments : null
            );

            try
            {
                await apiInstance.SendTransacEmailAsync(email);
            }
            catch (ApiException apiEx)
            {
                Debug.WriteLine($"Brevo API error: {apiEx.ErrorCode} - {apiEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error sending Brevo email: {ex.Message}");
                throw;
            }
        }




    }
}
