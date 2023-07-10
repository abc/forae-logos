using System.Collections.Immutable;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Html;
using IConfiguration = AngleSharp.IConfiguration;

namespace Kortos.Web;

public class NethysService
{
    private const string ContentDirName = "Content";
    private static IEnumerable<string> ValidFileExtensions => new List<string>()
    {
        ".jpg",
        ".png",
        ".jpeg",
        ".gif",
        ".tga",
        ".tif",
        ".tiff",
        ".webp",
        ".avif",
        ".apng"
    }.AsReadOnly();
    
    private readonly HttpClient _httpClient;
    private readonly HtmlParser _parser;
    private readonly string _contentPath;
    

    public NethysService(HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
    {
        _httpClient = httpClient;
        _parser = new HtmlParser();
        _contentPath = Path.Join(webHostEnvironment.WebRootPath, ContentDirName);
        _httpClient.BaseAddress = new Uri("https://2e.aonprd.com/");
    }

    private string GetLocalImagePath(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("Image path can't be empty", nameof(imagePath));
        }
        try
        {
            // Path.GetFullPath does some verification for us and throws an exception if any checks fail.
            var localPath = Path.GetFullPath(Path.Join(_contentPath, imagePath));
            // Check that the file has a good image extension.
            var extension = Path.GetExtension(localPath);
            if (!ValidFileExtensions.Contains(extension))
            {
                throw new ArgumentException($"{extension} is not an accepted file extension", nameof(imagePath));
            }
            // Check that the local path is a sub-path of the content root (i.e. the content root contains the new file) 
            if (Path.GetRelativePath(_contentPath, localPath).StartsWith('.'))
            {
                throw new InvalidOperationException("Directory traversal is not allowed");
            }
            return localPath;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Image path ({imagePath}) wasn't a valid image", nameof(imagePath), ex);
        }
    }

    private async Task DownloadAndReplaceImage(IHtmlImageElement image, CancellationToken cancel)
    {
        var srcAttribute = image.GetAttribute("src");
        image.SetAttribute("src", Path.Join("\\", ContentDirName, srcAttribute));
        
        if (string.IsNullOrWhiteSpace(srcAttribute))
        {
            return;
        }

        // Figure out what the local file path would be for the image.
        var localPath = GetLocalImagePath(srcAttribute);

        // Don't need to download & save if we already have it.
        if (File.Exists(localPath))
        {
            return;
        }

        // Create the directory structure and the file itself for the new image.
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        await using var newImageStream = File.Create(localPath);
        
        // Download and save the image to disk.
        var request = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = srcAttribute
        };
        var stream = await _httpClient.GetStreamAsync(request.Uri, cancel);
        // var localFile = _webHostEnvironment.WebRootFileProvider.GetFileInfo(srcAttribute);
        
        await stream.CopyToAsync(newImageStream, cancel);
    } 

    public async Task<string> GetEquipmentHtml(int itemId, CancellationToken cancel)
    {
        var requestBuilder = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = "Equipment.aspx",
            Query = $"ID={itemId}"
        };
        
        // GET and parse the HTML.
        var responseStream = await _httpClient.GetStreamAsync(requestBuilder.Uri, cancel);
        var document = await _parser.ParseDocumentAsync(responseStream, cancel);

        // Find the actual content we're actually interested in.
        var element = document.All
            .Single(e => e.Id == "ctl00_RadDrawer1_Content_MainContent_DetailedOutput");
        
        // Download and save all images locally, and replace the image's src tag to point at our local copy.
        await Task.WhenAll(element
            .GetNodes<IHtmlImageElement>()
            .Select(image => DownloadAndReplaceImage(image, cancel)));
        
        // Replace links to fully-qualified Archive of Nethys URLs
        foreach (var linkElement in element.GetNodes<IHtmlAnchorElement>())
        {
            linkElement.Href = Path.Join(_httpClient.BaseAddress!.AbsoluteUri, linkElement.GetAttribute("href"));
        }

        // Return the HTML of the content.
        return element
            .InnerHtml;
    }
}