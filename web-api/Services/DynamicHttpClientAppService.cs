using ACMS.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ACMS.WebApi.Services;

public class DynamicHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DynamicHttpClientService> _logger;

    public DynamicHttpClientService(HttpClient httpClient, ILogger<DynamicHttpClientService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Public Methods
    public async Task<JObject> CreateHttpClientAsync(CreateHttpClientDto input)
    {
        try
        {
            // Deserialize request and response configurations
            var requestConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(input.RequestConfig);

            // Construct URL with query parameters
            var url = BuildUrlWithQueryParams(requestConfig);

            _logger.LogInformation("Constructed URL: {Url}", url);

            // Create HTTP request
            var request = CreateHttpRequestMessage(requestConfig, url);

            // Send the HTTP request
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response: {ResponseBody}", responseBody);

            // If the response is a JSON array, parse it as a JArray
            var parsedResponse = JToken.Parse(responseBody);

            if (parsedResponse is JArray responseArray)
            {
                // You can map the array response into a JObject if needed, or return the JArray directly
                // For example, we return the array as a JObject for now.
                return JObject.FromObject(new { data = responseArray });
            }
            else if (parsedResponse is JObject responseObject)
            {
                // If the response is a single object, handle it as a JObject
                return responseObject;
            }

            // If neither, log and return an empty JObject or handle the case as needed
            _logger.LogWarning("Unexpected response format.");
            throw new Exception("Unexpected response format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            throw new ApplicationException("An unexpected error occurred.", ex);
        }
    }

    #endregion

    #region Private Methods

    private string BuildUrlWithQueryParams(Dictionary<string, object> requestConfig)
    {
        var url = requestConfig["url"]?.ToString() ?? string.Empty;
        if (requestConfig.ContainsKey("queryParams"))
        {
            var queryStringParams = ((JObject)requestConfig["queryParams"]).ToObject<Dictionary<string, object>>();
            var queryParams = queryStringParams?.Select(param => $"{param.Key}={param.Value}")
                                                 .ToList();

            if (queryParams?.Any() == true)
            {
                url = $"{url}?{string.Join("&", queryParams)}";
            }
        }

        return url;
    }

    private HttpRequestMessage CreateHttpRequestMessage(Dictionary<string, object> requestConfig, string url)
    {
        var method = requestConfig["httpMethod"]?.ToString() ?? "GET";
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        AddHeadersToRequest(requestConfig, request);
        AddContentToRequest(requestConfig, request);

        return request;
    }

    private void AddHeadersToRequest(Dictionary<string, object> requestConfig, HttpRequestMessage request)
    {
        if (requestConfig.ContainsKey("headers"))
        {
            var headers = ((JObject)requestConfig["headers"]).ToObject<Dictionary<string, object>>();
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value.ToString());
            }
        }
    }

    private void AddContentToRequest(Dictionary<string, object> requestConfig, HttpRequestMessage request)
    {
        if (requestConfig.ContainsKey("content-type") && !string.IsNullOrEmpty(requestConfig["content-type"]?.ToString()))
        {
            if (requestConfig.ContainsKey("body") && !string.IsNullOrEmpty(requestConfig["body"]?.ToString()))
            {
                var contentType = requestConfig["content-type"].ToString();
                var body = requestConfig["body"].ToString();
                var content = new StringContent(body, Encoding.UTF8, contentType);
                request.Content = content;
            }
        }
    }
    #endregion
}
