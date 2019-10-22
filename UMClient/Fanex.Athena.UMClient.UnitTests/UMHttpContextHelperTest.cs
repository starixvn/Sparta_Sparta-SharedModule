using System;
using Microsoft.AspNetCore.Http;
using Moq;
using ONEbook.UM.Helpers;
using Xunit;

namespace Fanex.Athena.UMClient.UnitTests
{
    public class UMHttpContextHelperTest
    {
        [Fact]
        public void GetSession_WhenGet_ReturnValueFromCache()
        {
            // Arrange
            var mockCache = new Mock<IUMCacheService>();
            mockCache.Setup(r => r.Get<string>("my-test-key")).Returns("test-value");
            var contextHelper = new UMHttpContextHelper(mockCache.Object, new Mock<IHttpContextAccessor>().Object);

            // Action
            var actual = contextHelper.GetSession("my-test-key");

            // Assert
            Assert.Equal("test-value", actual);
        }

        [Fact]
        public void SetSession_WhenCall_SetValueToCache()
        {
            // Arrange
            var mockCache = new Mock<IUMCacheService>();
            var contextHelper = new UMHttpContextHelper(mockCache.Object, new Mock<IHttpContextAccessor>().Object);

            // Action
            contextHelper.SetSession("test", "123-alo-ola");

            // Assert
            mockCache.Verify(r => r.Set("test", "123-alo-ola", TimeSpan.FromMinutes(1)));
        }

        [Fact]
        public void GetValueFromQueryString_WhenCall_ReturnValueFromRequestQuery()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockQuery = new Mock<IQueryCollection>();

            mockQuery.SetupGet(r => r["query-key"]).Returns("query-value");
            mockRequest.SetReturnsDefault(mockQuery.Object);
            mockHttpContext.SetReturnsDefault(mockRequest.Object);
            mockHttpContextAccessor.SetReturnsDefault(mockHttpContext.Object);
            var contextHelper = new UMHttpContextHelper(new Mock<IUMCacheService>().Object, mockHttpContextAccessor.Object);

            // Action
            var actual = contextHelper.GetValueFromQueryString("query-key");

            // Assert
            Assert.Equal("query-value", actual);
        }
    }
}