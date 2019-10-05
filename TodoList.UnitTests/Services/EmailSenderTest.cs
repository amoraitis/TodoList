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
        private readonly EmailSender _emailSenderService;
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly Mock<HttpContent> _httpContentMock;

        public EmailSenderTest()
        {
            _sendGridClientMock = new Mock<ISendGridClient>();
            _httpContentMock = new Mock<HttpContent>();

            _sendGridClientMock
                .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(System.Net.HttpStatusCode.Accepted, _httpContentMock.Object, null));

            _emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage());
        }

        [Fact]
        public async Task It_Should_Send_Email()
        {
            // This test doesn't make any assertion, basically here we're checking no assertion is thrown
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
        }

        [Fact]
        public async Task Should_Not_Throw_Exception_If_Can_Not_Send_Email()
        {
            _sendGridClientMock
               .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Response(System.Net.HttpStatusCode.InternalServerError, _httpContentMock.Object, null));

            EmailSender _emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage());
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
        }

        // TODO: Implement Should_Not_Throw_Exception_If_Can_Not_Send_Email() test
    }
}
