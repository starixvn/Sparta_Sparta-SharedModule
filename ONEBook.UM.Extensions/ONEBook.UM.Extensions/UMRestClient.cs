using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;

namespace ONEbook.UM.Extensions
{
    public class UMRestClient : IUMRestClient
    {
        public const string UMClientName = "UMClient";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UMClientConfiguration _umConfiguration;
        private readonly IPHelper _ipHelper;

        public UMRestClient(
            UMClientConfiguration configuration,
            IPHelper ipHelper,
            IHttpClientFactory httpClientFactory)
        {
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

            using (var client = _httpClientFactory.CreateClient(UMClientName))
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
                        request.Content = new StringContent(Serializer.Serialize(data), Encoding.UTF8, "application/json");
                    }

                    var response = await client.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
                    try
                    {
                        using (response)
                        {
                            response.EnsureSuccessStatusCode();
                            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return Serializer.Deserialize<T>(content);
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
    }
}