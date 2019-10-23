using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using Xunit;

namespace ONEBook.UM.Extensions.UnitTests
{
    public class UMRestClientTest
    {
        private Mock<IPHelper> _mockIPHelper;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpClient> _mockClient;
        private UMClientConfiguration _fakeUMClientConfiguration;

        public UMRestClientTest()
        {
            _mockIPHelper = new Mock<IPHelper>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockClient = new Mock<HttpClient>();
            _fakeUMClientConfiguration = new UMClientConfiguration();
            _mockClient.Object.BaseAddress = new Uri("http://test.com");
        }

        [Fact]
        public async void ExecutePostAsync_WhenCall_RequestPostWithData()
        {
            // Arrange
            var testTime = new DateTime(2000, 1, 2, 3, 4, 5, 6);
            HttpRequestMessage actual = null;
            string messageContent = null;
            _mockClient.Object.BaseAddress = new Uri("http://test.com/v2");
            _mockHttpClientFactory.Setup(r => r.CreateClient("UMClient")).Returns(_mockClient.Object);
            _mockClient.Setup(r => r.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .ReturnsAsync(new HttpResponseMessage { Content = new StringContent("{}") })
                .Callback<HttpRequestMessage, CancellationToken>(async (request, _) =>
                {
                    actual = request;
                    messageContent = await request.Content.ReadAsStringAsync();
                })
                .Verifiable();

            var um = CreateUMRestClient();

            // Action
            await um.ExecutePostAsync<SampleTestClass>("/test/456", new SampleTestClass { Time = testTime });

            // Assert
            Assert.Equal(HttpMethod.Post, actual.Method);
            Assert.Equal("http://test.com/v2/test/456", actual.RequestUri.OriginalString);
            Assert.Equal("{\"Time\":\"2000-01-02T03:04:05.006\"}", messageContent);
        }

        [Fact]
        public async void ExecuteGetAsync_WhenCall_RequestGetWithoutData()
        {
            // Arrange
            HttpRequestMessage actual = null;
            _mockClient.Object.BaseAddress = new Uri("http://test.com/v1/api");
            _mockHttpClientFactory.Setup(r => r.CreateClient("UMClient")).Returns(_mockClient.Object);
            _mockClient.Setup(r => r.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .ReturnsAsync(new HttpResponseMessage { Content = new StringContent("{}") })
                .Callback<HttpRequestMessage, CancellationToken>((a, _) => actual = a)
                .Verifiable();

            var um = CreateUMRestClient();

            // Action
            await um.ExecuteGetAsync<SampleTestClass>("/test/123");

            // Assert
            Assert.Equal(HttpMethod.Get, actual.Method);
            Assert.Equal("http://test.com/v1/api/test/123", actual.RequestUri.OriginalString);
            Assert.Null(actual.Content);
        }

        [Fact]
        public async void ExecuteGetAsync_RequestNotOk_Throws()
        {
            // Arrange
            _mockHttpClientFactory.SetReturnsDefault(_mockClient.Object);
            _mockClient.Setup(r => r.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadGateway)
                {
                    Content = new StringContent("content error"),
                    ReasonPhrase = "test-response-error"
                });

            var um = CreateUMRestClient();

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => um.ExecuteGetAsync<SampleTestClass>("/test"));

            // Assert
            Assert.Equal("Exception on UMRestClient.ExecuteAsync", actual.Message);
            Assert.NotNull(actual.InnerException);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async void ExecuteGetAsync_EnpointInvalid_Throws(string endpoint)
        {
            // Arrange
            var um = CreateUMRestClient();

            // Action
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => um.ExecuteGetAsync<SampleTestClass>(endpoint));
        }

        [Fact]
        public async void ExecuteGetAsync_ResponseContent_Deserialize()
        {
            // Arrange
            _mockHttpClientFactory.SetReturnsDefault(_mockClient.Object);
            _mockClient.Setup(r => r.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .ReturnsAsync(new HttpResponseMessage { Content = new StringContent("{\"Time\":\"2012-04-23T18:25:43.511Z\"}") });

            var um = CreateUMRestClient();

            // Action
            var actual = await um.ExecuteGetAsync<SampleTestClass>("/test");

            // Assert
            Assert.Equal(new DateTime(2012, 4, 23, 18, 25, 43, 511), actual.Time);
        }

        [Fact]
        public async void ExecuteGetAsync_Authorization_AddToHeader()
        {
            // Arrange
            HttpRequestMessage request = null;
            _mockHttpClientFactory.SetReturnsDefault(_mockClient.Object);
            _mockClient.Setup(r => r.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .ReturnsAsync(new HttpResponseMessage { Content = new StringContent("{}") })
                .Callback<HttpRequestMessage, CancellationToken>((a, _) => request = a);

            var um = CreateUMRestClient();

            // Action
            await um.ExecuteGetAsync<SampleTestClass>("/test");

            // Assert
            Assert.NotEmpty(request.Headers.GetValues("Authorization").Single());
        }

        [Fact]
        public void ExecuteGet_WhenCall_ThrowsNotSupport()
        {
            // Arrange
            var um = CreateUMRestClient();

            // Action
            // Assert
            Assert.Throws<NotSupportedException>(() => um.ExecuteGet<SampleTestClass>("any"));
        }

        [Fact]
        public void ExecutePost_WhenCall_ThrowsNotSupport()
        {
            // Arrange
            var um = CreateUMRestClient();

            // Action
            // Assert
            Assert.Throws<NotSupportedException>(() => um.ExecutePost<SampleTestClass>("any"));
        }

        private UMRestClient CreateUMRestClient()
        {
            return new UMRestClient(
                _fakeUMClientConfiguration,
                _mockIPHelper.Object,
                _mockHttpClientFactory.Object);
        }

        public class SampleTestClass
        {
            public DateTime Time { get; set; }
        }
    }
}