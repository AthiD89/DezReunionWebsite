using System.Text.RegularExpressions;

namespace DezReunionWebsite.Services;

public class TikTokLinkService
{
    private static readonly Regex VideoIdPattern = new(@"/video/(\d+)", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;

    public TikTokLinkService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string CanonicalUrl, string VideoId)?> ResolveAsync(string url)
    {
        var directMatch = VideoIdPattern.Match(url);
        if (directMatch.Success)
        {
            return (url, directMatch.Groups[1].Value);
        }

        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
            var finalMatch = VideoIdPattern.Match(finalUrl);
            return finalMatch.Success ? (finalUrl, finalMatch.Groups[1].Value) : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
