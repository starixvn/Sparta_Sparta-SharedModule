using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;

namespace ONEBook.UM.Extensions
{
    public class UMRestClient : IUMRestClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UMClientConfiguration _umConfiguration;
        private readonly IPHelper _ipHelper;
        private readonly string _umClientName;

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        public UMRestClient(
            string clientName,
            UMClientConfiguration configuration,
            IPHelper ipHelper,
            IHttpClientFactory httpClientFactory)
        {
            _umClientName = clientName;
            _ipHelper = ipHelper;
            _umConfiguration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> ExecuteGetAsync<T>(string endPoint)
        {
            return await ExecuteAsync<T>(endPoint, HttpMethod.Get, null).ConfigureAwait(false);
        }

        public async Task<T> ExecutePostAsync<T>(string endPoint, object data = null)
        {
            return await ExecuteAsync<T>(endPoint, HttpMethod.Post, data).ConfigureAwait(false);
        }

        public T ExecuteGet<T>(string endPoint) where T : new()
        {
            throw new NotSupportedException();
        }

        public T ExecutePost<T>(string endPoint, object data = null) where T : new()
        {
            throw new NotSupportedException();
        }

        private async Task<T> ExecuteAsync<T>(string endPoint, HttpMethod method, object data)
        {
            ValidateEndPoint(endPoint);

            using (var client = _httpClientFactory.CreateClient(_umClientName))
            {
                using (var request = new HttpRequestMessage(method, new Uri(client.BaseAddress + endPoint)))
                {
                    request.Headers.Add("Authorization", HttpClientHelper.GenerateAuthorizationHeader(
                        _umConfiguration,
                        request.RequestUri.OriginalString,
                        request.Method.ToString(),
                        _ipHelper.GetMachineIP()));

                    if (data != null)
                    {
                        request.Content = CreateJsonContent(data);
                    }

                    var response = await client.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
                    try
                    {
                        using (response)
                        {
                            response.EnsureSuccessStatusCode();
                            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return Deserialize<T>(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        response?.Dispose();
                        throw new Exception("Exception on UMRestClient.ExecuteAsync", ex);
                    }
                }
            }

        }

        private static void ValidateEndPoint(string endPoint)
        {
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
        }

        private static HttpContent CreateJsonContent(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    var serializer = JsonSerializer.Create(_serializerSettings);
                    serializer.Serialize(jsonWriter, obj);
                    return new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/json");
                }
            }
        }

        private static T Deserialize<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    var serializer = JsonSerializer.Create(_serializerSettings);
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}