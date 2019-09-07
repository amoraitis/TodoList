using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using TodoList.Web.Services;
using Xunit;

namespace TodoList.UnitTests.Services
{
    public class EmailSenderTest
    {
        private EmailSender _emailSenderService;
        private Mock<ISendGridClient> _sendGridClient;
        private Mock<HttpContent> _httpContent;

        public EmailSenderTest()
        {
            _sendGridClient = new Mock<ISendGridClient>();
            _httpContent = new Mock<HttpContent>();

            _sendGridClient
                .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(System.Net.HttpStatusCode.Accepted, _httpContent.Object, null));

            _emailSenderService = new EmailSender(_sendGridClient.Object, new SendGridMessage());
        }

        [Fact]
        public async Task It_Should_Send_Email()
        {
            // This test doesn't make any assertion, basically here we're checking no assertion is thrown
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
        }

        // TODO: Implement Should_Not_Throw_Exception_If_Can_Not_Send_Email() test
    }
}
