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

app.MapPost("pdf", async (IFileService fileService, IPuppeteerService puppeteerService, HttpRequest request) =>
{
    if (!request.Form.Files.Any())
        return Results.BadRequest("No uploaded files!");

    if (request.Form.Files.Count > 1)
        return Results.BadRequest("Too many files uploaded. Upload limit = 1");

    IFormFile file = request.Form.Files[0];

    (FileModel? processedFile, string? ErrorMessage) = await fileService.ProcessAsync(file);

    if (processedFile is null)
        return Results.BadRequest(ErrorMessage);

    if (string.IsNullOrEmpty(processedFile.Content))
        return Results.BadRequest("Empty file!");

    (Stream? pdfStream, ErrorMessage) = await puppeteerService.ConvertAsync(processedFile.Content);

    if (pdfStream is null)
        return Results.Problem(ErrorMessage);

    return Results.File(pdfStream, contentType: "application/pdf");
});

IPuppeteerService puppeteerService = app.Services.GetRequiredService<IPuppeteerService>();
await puppeteerService.InitializeAsync();

app.Run();