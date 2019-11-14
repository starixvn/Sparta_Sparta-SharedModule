using System;
using System.Threading.Tasks;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.NonWebApp.Configurations;
using RestSharp;

namespace ONEbook.UM.NonWebApp
{
    public class UMRestClient : IUMRestClient
    {
        private static UMRestClient _instance = null;
        private static readonly object padlock = new object();

        private readonly IRestClient _httpClient;
        private readonly UMClientConfiguration _umConfiguration;
        private readonly IPHelper _ipHelper;

        public UMRestClient(
            UMClientConfiguration configuration,
            IPHelper ipHelper,
            IRestClient httpClient)
        {
            _ipHelper = ipHelper;
            _umConfiguration = configuration;
            _httpClient = httpClient;
        }

        public static UMRestClient Instance
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new UMRestClient(UMClientConfigurationSection.Settings, new IPHelperImpl(), new RestClient()));
                }
            }
        }

        public Task<T> ExecuteGetAsync<T>(string endPoint) => ExecuteAsync<T>(endPoint, Method.GET, null);

        public Task<T> ExecutePostAsync<T>(string endPoint, object data = null) => ExecuteAsync<T>(endPoint, Method.POST, data);

        public T ExecuteGet<T>(string endPoint) where T : new() => Execute<T>(endPoint, Method.GET, null);

        public T ExecutePost<T>(string endPoint, object data = null) where T : new() => Execute<T>(endPoint, Method.POST, data);

        private async Task<T> ExecuteAsync<T>(string endPoint, Method method, object data)
        {
            ValidateEndPoint(endPoint);

            var request = PrepareRequestMessage(endPoint, method, data);
            var response = await _httpClient.ExecuteTaskAsync<T>(request).ConfigureAwait(false);

            if (!response.IsSuccessful)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return response.Data;
        }

        private T Execute<T>(string endPoint, Method method, object data) where T : new()
        {
            ValidateEndPoint(endPoint);

            var request = PrepareRequestMessage(endPoint, method, data);
            var response = _httpClient.Execute<T>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return response.Data;
        }

        private static void ValidateEndPoint(string endPoint)
        {
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
        }

        private IRestRequest PrepareRequestMessage(string endPoint, Method method, object data)
        {
            var requestUrl = _umConfiguration.ServiceUri + endPoint;
            var request = new RestRequest(requestUrl, method);

            if (data != null)
            {
                request.AddJsonBody(data);
            }

            request.AddHeader("Authorization", HttpClientHelper.GenerateAuthorizationHeader(
                _umConfiguration,
                requestUrl,
                request.Method.ToString(),
                _ipHelper.GetMachineIP()));

            return request;
        }
    }
}