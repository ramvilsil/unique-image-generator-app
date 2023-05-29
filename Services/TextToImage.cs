using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Application.Services;

public class TextToImage
{
    private const string ApiUrl = "https://api.openai.com/v1/images/generations";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private void ConfigureHttpClient(IConfiguration configuration)
    {
        var apiKey = configuration.GetValue<string>("OpenAIApi:SecretKey");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public TextToImage
    (
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment env
    )
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        ConfigureHttpClient(configuration);
        _env = env;
    }

    public async Task<string?> GenerateImageFromText(string inputText)
    {
        var requestBody = new
        {
            prompt = inputText,
            n = 1,
            size = "1024x1024",
            response_format = "url"
        };

        using var response = await _httpClient.PostAsync(ApiUrl, new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        if (response.StatusCode != HttpStatusCode.OK) return null;

        var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

        if (result == null) return null;

        string generatedImageUrl = Convert.ToString(result.data[0].url);

        return generatedImageUrl;
    }
}