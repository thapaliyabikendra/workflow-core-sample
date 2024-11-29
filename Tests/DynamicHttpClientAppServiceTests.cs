using ACMS.WebApi.Models;
using ACMS.WebApi.Services;
using Newtonsoft.Json.Linq;

namespace ACMS.WebApi.Tests;

public class DynamicHttpClientAppServiceTests
{
    public async Task TestAsync()
    {
        // Sample RequestConfig and ResponseConfig JSON strings
        var requestConfigJson = new JObject
        {
            { "url", "https://jsonplaceholder.typicode.com/posts" },
            { "httpMethod", "GET" },
            { "queryParams", new JObject { { "userId", 1 } } },
            { "headers", new JObject { { "Accept", "application/json" } } }
        }.ToString();

        // Create an instance of CreateHttpClientDto with the sample data
        var inputDto = new CreateHttpClientDto
        {
            RequestConfig = requestConfigJson
        };

        // Instantiate HttpClient and Logger (In a real app, these would be injected via dependency injection)
        var httpClient = new HttpClient();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DynamicHttpClientService>();

        // Instantiate the service
        var dynamicHttpClientService = new DynamicHttpClientService(httpClient, logger);

        // Call the CreateHttpClientAsync method with the sample data
        var result = await dynamicHttpClientService.CreateHttpClientAsync(inputDto);

        // Output the result (assuming you want to see the response configuration object)
        Console.WriteLine("Response Config: ");
        Console.WriteLine(result.ToString());
    }
}
