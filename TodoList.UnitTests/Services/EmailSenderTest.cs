using System;
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
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
        }

        [Fact]
        public async Task Should_Not_Throw_Exception_If_Can_Not_Send_Email()
        {
            //Arrange
            _sendGridClientMock
               .Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Response(HttpStatusCode.InternalServerError, _httpContentMock.Object, null));

            EmailSender _emailSenderService = new EmailSender(_sendGridClientMock.Object, new SendGridMessage());
            //Act
            await _emailSenderService.SendEmailAsync("max@example.com", "This is a test", "A test body");
            // This test doesn't make any assertion, basically here we're checking no exception is thrown
        }
    }
}
