using Microsoft.AspNetCore.Mvc;
using Application.ViewModels;
using Application.Services;

namespace Application.Controllers;

public class MainController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private readonly TextToImage _textToImage;
    private readonly ImageToVariant _imageToVariant;

    private void WriteToTextFile(string text)
    {
        string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/GeneratedImages.txt");
        System.IO.File.AppendAllText(filePath, text);
    }

    public MainController
    (
        IConfiguration configuration,
        IHostEnvironment env,
        TextToImage textToImage,
        ImageToVariant imageToVariant
    )
    {
        _configuration = configuration;
        _env = env;
        _textToImage = textToImage;
        _imageToVariant = imageToVariant;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult TextToImage() => View();

    [HttpGet]
    public IActionResult ImageToVariant() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateImageFromText(TextToImageViewModel viewModel)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (viewModel.AppPassword != _configuration.GetValue<string>("App:Password")) return RedirectToAction(nameof(TextToImage));

        if (string.IsNullOrWhiteSpace(viewModel.InputText)) return BadRequest();

        var generatedImageUrl = await _textToImage.GenerateImageFromText(viewModel.InputText);

        if (string.IsNullOrWhiteSpace(generatedImageUrl)) return BadRequest();

        WriteToTextFile($"\nImage From Text, {viewModel.InputText}: {generatedImageUrl}\n");

        return RedirectToAction(nameof(Image), new { imageUrl = generatedImageUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateImageVariant(ImageToVariantViewModel viewModel)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (viewModel.AppPassword != _configuration.GetValue<string>("App:Password")) return RedirectToAction(nameof(ImageToVariant));

        var generatedImageUrl = await _imageToVariant.GenerateImageVariant(viewModel.ImageFile);

        if (string.IsNullOrWhiteSpace(generatedImageUrl)) return BadRequest();

        WriteToTextFile($"\nImage Variant: {generatedImageUrl}\n");

        return RedirectToAction(nameof(Image), new
        {
            imageUrl = generatedImageUrl
        });
    }

    [HttpGet]
    public IActionResult Image(string imageUrl)
    {
        var viewModel = new ImageViewModel { ImageUrl = imageUrl };
        return View(viewModel);
    }

}