using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using ONEBook.UM.Extensions.Middleware;
using Xunit;

namespace ONEBook.UM.Extensions.UnitTests.Middleware
{
    public class UMHandlerTest
    {
        private readonly Mock<IMaintenanceService> _mockMaintenanceService;
        private readonly Mock<HttpContext> _mockContext;
        private readonly Mock<HttpRequest> _mockRequest;

        public UMHandlerTest()
        {
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _mockContext = new Mock<HttpContext>();
            _mockRequest = new Mock<HttpRequest>();
            _mockContext.SetupGet(r => r.Request).Returns(_mockRequest.Object);
        }

        public static IEnumerable<object[]> SkipCheckingUMPaths
        {
            get
            {
                yield return new object[] { PathString.Empty };
                yield return new object[] { PathString.FromUriComponent("/test/error/404") };
            }
        }

        public static IEnumerable<object[]> AjaxRequestHeaders
        {
            get
            {
                var mockHeader = new Mock<IHeaderDictionary>();
                mockHeader.SetupGet(r => r["X-Requested-With"]).Returns("XMLHttpRequest");
                yield return new[] { mockHeader.Object };

                mockHeader.Reset();
                mockHeader.SetupGet(r => r["Accept"]).Returns("*/*");
                mockHeader.SetupGet(r => r["Sec-Fetch-Mode"]).Returns("cors");
                yield return new[] { mockHeader.Object };
            }
        }

        [Theory]
        [MemberData(nameof(SkipCheckingUMPaths))]
        public async Task Invoke_NoPathOrErrorPage_InvokeNext(PathString path)
        {
            // Arrange
            bool isNextHandlerInvoked = false;
            _mockRequest.SetupGet(r => r.Path).Returns(path);
            var handler = new UMHandler(new RequestDelegate(_ =>
            {
                isNextHandlerInvoked = true;
                return Task.CompletedTask;
            }), _mockMaintenanceService.Object);

            // Action
            await handler.Invoke(_mockContext.Object);

            // Assert
            Assert.True(isNextHandlerInvoked);
        }

        [Fact]
        public async Task Invoke_NotUM_InvokeNext()
        {
            // Arrange
            bool isNextHandlerInvoked = false;
            _mockRequest.SetupGet(r => r.Path).Returns(PathString.FromUriComponent("/hello"));
            var handler = new UMHandler(new RequestDelegate(_ =>
            {
                isNextHandlerInvoked = true;
                return Task.CompletedTask;
            }), _mockMaintenanceService.Object);
            _mockMaintenanceService.Setup(r => r.IsUnderMaintenanceAsync()).ReturnsAsync(false);

            // Action
            await handler.Invoke(_mockContext.Object);

            // Assert
            Assert.True(isNextHandlerInvoked);
        }

        [Fact]
        public async Task Invoke_IsUM_HtmlMessage()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var memoryStream = new MemoryStream();

            mockResponse.SetupGet(r => r.Body).Returns(memoryStream);
            _mockContext.SetupGet(r => r.Response).Returns(mockResponse.Object);
            _mockRequest.SetupGet(r => r.Path).Returns(PathString.FromUriComponent("/test"));
            _mockMaintenanceService.Setup(r => r.IsUnderMaintenanceAsync()).ReturnsAsync(true);
            _mockMaintenanceService.Setup(r => r.GetUnderMaintenanceTimeAsync()).ReturnsAsync(new UnderMaintenanceTime
            {
                From = "12:34:56PM",
                To = "22:33:44PM"
            });

            var handler = new UMHandler(new RequestDelegate(_ => Task.CompletedTask), _mockMaintenanceService.Object);

            // Action
            await handler.Invoke(_mockContext.Object);
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());
            memoryStream.Dispose();

            // Assert
            var expectation = "<div style='text-align:center;padding-top:10%'>" +
                "System is currently unavailable due to scheduled maintenance.<br /> " +
                "This downtime will last from <b>12:34:56PM</b> &rarr; <b>22:33:44PM</b>. " +
                "Thanks for your patience!</div>";
            Assert.Equal(expectation, actual);
            mockResponse.VerifySet(r => r.ContentType = "text/html");
        }

        [Theory]
        [MemberData(nameof(AjaxRequestHeaders))]
        public async Task Invoke_UMWithAjaxrequest_JsonMessage(IHeaderDictionary headers)
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var memoryStream = new MemoryStream();

            mockResponse.SetupGet(r => r.Body).Returns(memoryStream);
            _mockContext.SetupGet(r => r.Response).Returns(mockResponse.Object);
            _mockRequest.SetupGet(r => r.Path).Returns(PathString.FromUriComponent("/test-ajax"));
            _mockRequest.SetupGet(r => r.Headers).Returns(headers);

            _mockMaintenanceService.Setup(r => r.IsUnderMaintenanceAsync()).ReturnsAsync(true);
            _mockMaintenanceService.Setup(r => r.GetUnderMaintenanceTimeAsync()).ReturnsAsync(new UnderMaintenanceTime
            {
                From = "11:22:33AM",
                To = "01:23:45PM"
            });

            var handler = new UMHandler(new RequestDelegate(_ => Task.CompletedTask), _mockMaintenanceService.Object);

            // Action
            await handler.Invoke(_mockContext.Object);
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());
            memoryStream.Dispose();

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                IsSuccess = false,
                Message = "System is currently unavailable due to scheduled maintenance.\r\n" +
                          "This downtime will last from 11:22:33AM to 01:23:45PM."
            }), actual);
            mockResponse.VerifySet(r => r.ContentType = "application/json");
        }
    }
}