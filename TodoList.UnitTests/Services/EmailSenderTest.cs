using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TodoList.Core.Services;
using Xunit;

namespace TodoList.UnitTests.Services
{
    public class EmailSenderTest
    {
        private readonly EmailSender _emailSenderService;
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly Mock<HttpContent> _httpContentMock;
        private readonly NullLogger<EmailSender> _logger;
        private const string email = "max@example.com";
        private const string subject = "This is a test";
        private const string body = "A test body";

        public EmailSenderTest()
        {
            _sendGridClientMock = new Mock<ISendGridClient>();
            _httpContentMock = new Mock<HttpContent>();
            _logger = new NullLogger<EmailSender>();

            _sendGridClientMock
                .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.Accepted, _httpContentMock.Object, null));

            _emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage(),_logger);
        }

        [Fact]
        public async Task It_Should_Send_Email()
        {
            // This test doesn't make any assertion, basically here we're checking no assertion is thrown
            await _emailSenderService.SendEmailAsync(email,subject,body);
        }

        [Fact]
        public async Task Should_Not_Throw_Exception_If_Can_Not_Send_Email()
        {
            //Arrange
            _sendGridClientMock
               .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Response(HttpStatusCode.InternalServerError, _httpContentMock.Object, null));

            EmailSender emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage(),_logger);
            //Act
            await emailSenderService.SendEmailAsync(email, subject, body);
            // This test doesn't make any assertion, basically here we're checking no exception is thrown
        }
    }
}
