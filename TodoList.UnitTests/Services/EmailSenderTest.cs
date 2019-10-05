using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<EmailSender>> _logger;

        public EmailSenderTest()
        {
            _sendGridClientMock = new Mock<ISendGridClient>();
            _httpContentMock = new Mock<HttpContent>();
            _logger = new Mock<ILogger<EmailSender>>();

            _sendGridClientMock
                .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(System.Net.HttpStatusCode.Accepted, _httpContentMock.Object, null));

            _emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage(),_logger.Object);
        }

        [Fact]
        public async Task It_Should_Send_Email()
        {
            // This test doesn't make any assertion, basically here we're checking no assertion is thrown
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
        }
    }
}
