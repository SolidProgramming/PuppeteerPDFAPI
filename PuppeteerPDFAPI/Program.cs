global using PuppeteerPDFAPI.Models;
using PuppeteerPDFAPI.Services;


WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IPuppeteerService, PuppeteerService>();

builder.WebHost.UseKestrel(_ =>
{
    _.Limits.MaxRequestBodySize = 524288000;
});

WebApplication app = builder.Build();

app.MapPost("pdf", async (IFileService fileService, HttpRequest request) =>
{
    if (!request.Form.Files.Any())
        return Results.BadRequest("No uploaded files!");

    if (request.Form.Files.Count > 1)
        return Results.Problem("Too many files uploaded. Upload limit = 1");

    FileModel file = new()
    {
        Guid = Guid.NewGuid().ToString(),
        FileExtension = request.Form.Files[0].FileName,
        File = request.Form.Files[0]
    };


    await fileService.UploadAsync(file);

    return Results.Ok();
});

IPuppeteerService puppeteerService = app.Services.GetRequiredService<IPuppeteerService>();
await puppeteerService.InitializeAsync();

app.Run();