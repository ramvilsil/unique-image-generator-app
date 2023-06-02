using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Application.Services;

public class ImageToVariant
{
    const string ApiUrl = "https://api.openai.com/v1/images/variations";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private void ConfigureHttpClient(IConfiguration configuration)
    {
        var apiKey = configuration.GetValue<string>("OpenAIApi:SecretKey");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public ImageToVariant
    (
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory
    )
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        ConfigureHttpClient(configuration);
    }

    public async Task<string?> GenerateImageVariant(IFormFile imageFile)
    {
        if (imageFile.Length <= 0) return null;

        using var memoryStream = new MemoryStream();
        await imageFile.CopyToAsync(memoryStream);

        var pngHeaderBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var fileHeaderBytes = memoryStream.ToArray().Take(pngHeaderBytes.Length).ToArray();

        if (!pngHeaderBytes.SequenceEqual(fileHeaderBytes)) return null;

        memoryStream.Position = 0;

        var multipartFormDataContent = new MultipartFormDataContent();

        multipartFormDataContent.Add(new StreamContent(memoryStream), "image", imageFile.FileName);
        multipartFormDataContent.Add(new StringContent("1"), "n");
        multipartFormDataContent.Add(new StringContent("1024x1024"), "size");
        multipartFormDataContent.Add(new StringContent("url"), "response_format");

        using var response = await _httpClient.PostAsync(ApiUrl, multipartFormDataContent);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode != HttpStatusCode.OK) return null;

        var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

        if (result == null) return null;

        string generatedImageUrl = Convert.ToString(result.data[0].url);

        return generatedImageUrl;
    }

}