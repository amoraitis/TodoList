using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration Configuration;

        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(Configuration["SendGrid:ServiceApiKey"])) return;

            var apiKey = Configuration["SendGrid:ServiceApiKey"];

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress("noreply@amoraitis.todolist.com", "Amoraitis.TodoList Team"));
            msg.AddTo(new EmailAddress(email));
            msg.AddContent(MimeType.Html, message);
            msg.SetSubject(subject);

            var result = await client.SendEmailAsync(msg);
            
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                return;

        }
    }
}
