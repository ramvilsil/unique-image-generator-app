using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Application.ViewModels;

namespace Application.Controllers;

public class MainController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private const string ApiUrl = "https://api.openai.com/v1/images/generations";
    private void WriteToTextFile(string text)
    {
        string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/Inputs.txt");
        System.IO.File.AppendAllText(filePath, text);
    }
    private void ConfigureHttpClient(IConfiguration configuration)
    {
        var apiKey = configuration.GetValue<string>("OpenAIApi:SecretKey");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public MainController
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

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(TextToImage));

    [HttpGet]
    public IActionResult TextToImage() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateImageFromText(TextToImageViewModel viewModel)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (viewModel.AppPassword != _configuration.GetValue<string>("App:Password")) return RedirectToAction("Index");

        if (string.IsNullOrWhiteSpace(viewModel.InputText)) return BadRequest();

        var requestBody = new
        {
            prompt = viewModel.InputText,
            n = 1,
            size = "1024x1024",
            response_format = "url"
        };

        using var response = await _httpClient.PostAsync(ApiUrl, new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        if (response.StatusCode != HttpStatusCode.OK) return BadRequest();

        var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

        if (result == null) return BadRequest();

        string generatedImageUrl = Convert.ToString(result.data[0].url);

        WriteToTextFile($"\n{viewModel.InputText} : {generatedImageUrl}\n");

        return RedirectToAction(nameof(Image), new { imageUrl = generatedImageUrl });
    }

    [HttpGet]
    public IActionResult Image(string imageUrl)
    {
        var viewModel = new ImageViewModel { ImageUrl = imageUrl };
        return View(viewModel);
    }

}