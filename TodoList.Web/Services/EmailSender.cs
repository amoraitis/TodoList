using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TodoList.Web.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly ISendGridClient _client;
        private readonly SendGridMessage _message;

        public EmailSender(ISendGridClient sendGridClient, SendGridMessage sendGridMessage)
        {
            _client = sendGridClient;
            _message = sendGridMessage;

            _message.SetFrom(new EmailAddress("noreply@amoraitis.todolist.com", "TodoList Team"));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _message.AddTo(new EmailAddress(email));
            _message.AddContent(MimeType.Html, message);
            _message.SetSubject(subject);

            var result = await _client.SendEmailAsync(_message);
            
            if (result.StatusCode != System.Net.HttpStatusCode.Accepted) {
                throw new Exception("The email couldn't be sent.");
            }
        }
    }
}
