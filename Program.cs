using Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TextToImage>();
builder.Services.AddScoped<ImageToVariant>();

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}"
    );

app.Run();