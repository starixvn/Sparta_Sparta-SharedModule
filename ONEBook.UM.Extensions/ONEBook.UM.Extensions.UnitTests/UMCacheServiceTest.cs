using System;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ONEBook.UM.Extensions.UnitTests
{
    public class UMCacheServiceTest
    {
        private readonly Mock<IDistributedCache> _mockCache;

        public UMCacheServiceTest()
        {
            _mockCache = new Mock<IDistributedCache>();
        }

        [Fact]
        public void Get_NoDataInCache_ReturnDefault()
        {
            // Arrange
            _mockCache.SetReturnsDefault((byte[])null);
            var um = new UMCacheService("test-123", _mockCache.Object);

            // Action
            // Assert
            Assert.Equal(0, um.Get<int>("any-int"));
            Assert.Equal(DateTime.MinValue, um.Get<DateTime>("any-datetime"));
            Assert.Null(um.Get<string>("any-string"));
        }

        [Fact]
        public void Get_CacheHasData_Deserialize()
        {
            // Arrange
            var sampleData = new UMCacheSampleTestClass
            {
                Value1 = 123,
                Value2 = "test-um-cache",
                Value3 = new DateTime(2000, 3, 4)
            };
            var json = JsonConvert.SerializeObject(sampleData, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });
            _mockCache.Setup(r => r.Get("site-456UMSession_mytestkey")).Returns(Encoding.UTF8.GetBytes(json));
            var um = new UMCacheService("site-456", _mockCache.Object);

            // Action
            var actual = um.Get<UMCacheSampleTestClass>("mytestkey");

            // Assert
            Assert.Equal(sampleData.Value1, actual.Value1);
            Assert.Equal(sampleData.Value2, actual.Value2);
            Assert.Equal(sampleData.Value3, actual.Value3);
        }

        [Fact]
        public void Set_NoData_DoNothing()
        {
            // Arrange
            var um = new UMCacheService("app-789", _mockCache.Object);

            // Action
            um.Set("test", (string)null, TimeSpan.FromSeconds(1));

            // Assert
            _mockCache.Verify(r => r.Set(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>()), Times.Never());
        }

        [Fact]
        public void Set_HasData_SetCacheWithAbsoluteExpiration()
        {
            // Arrange
            var testObject = new UMCacheSampleTestClass
            {
                Value1 = 1,
                Value2 = "2",
                Value3 = new DateTime(3, 4, 5)
            };
            var json = JsonConvert.SerializeObject(testObject, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });
            var expectedData = Encoding.UTF8.GetBytes(json);

            DistributedCacheEntryOptions actualOptions = null;
            byte[] actualData = null;
            _mockCache.Setup(r => r.Set("web-111UMSession_test", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Callback<string, byte[], DistributedCacheEntryOptions>((key, value, options) =>
                {
                    actualOptions = options;
                    actualData = value;
                })
                .Verifiable();

            var expiry = TimeSpan.FromSeconds(66);
            var um = new UMCacheService("web-111", _mockCache.Object);

            // Action
            um.Set("test", testObject, expiry);

            // Assert
            _mockCache.Verify();
            Assert.Equal(expiry, actualOptions.AbsoluteExpirationRelativeToNow);
            Assert.Equal(expectedData, actualData);
        }

        public class UMCacheSampleTestClass
        {
            public int Value1 { get; set; }
            public string Value2 { get; set; }
            public DateTime Value3 { get; set; }
        }
    }
}