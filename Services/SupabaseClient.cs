using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DezReunionWebsite.Models;
using Microsoft.Extensions.Configuration;

namespace DezReunionWebsite.Services;

public class SupabaseClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex TikTokVideoIdPattern = new(@"tiktok\.com/.*?/video/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex YouTubeIdPattern = new(@"(?:youtube\.com/(?:watch\?v=|embed/|shorts/)|youtu\.be/)([a-zA-Z0-9_-]{11})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly HttpClient _http;
    private readonly SupabaseAuthStateProvider _auth;
    private readonly string _baseUrl;
    private readonly string _publishableKey;

    public SupabaseClient(HttpClient http, IConfiguration configuration, SupabaseAuthStateProvider auth)
    {
        _http = http;
        _auth = auth;
        _baseUrl = configuration["Supabase:Url"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Supabase:Url is not configured (wwwroot/appsettings.json).");
        _publishableKey = configuration["Supabase:PublishableKey"]
            ?? throw new InvalidOperationException("Supabase:PublishableKey is not configured (wwwroot/appsettings.json).");
    }

    // ---------------- Events ----------------

    public async Task<List<EventItem>> GetAllEventsAsync()
    {
        var response = await SendAsync(HttpMethod.Get, "/rest/v1/events?select=*&order=date.asc");
        return await ReadListAsync<EventItem>(response);
    }

    public async Task<List<EventItem>> GetUpcomingEventsAsync()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var response = await SendAsync(HttpMethod.Get, $"/rest/v1/events?select=*&date=gte.{today}&order=date.asc");
        return await ReadListAsync<EventItem>(response);
    }

    public async Task<List<EventItem>> GetPastEventsAsync()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var response = await SendAsync(HttpMethod.Get, $"/rest/v1/events?select=*&date=lt.{today}&order=date.desc");
        return await ReadListAsync<EventItem>(response);
    }

    public async Task<EventItem?> GetEventByIdAsync(int id)
    {
        var response = await SendAsync(HttpMethod.Get, $"/rest/v1/events?select=*&id=eq.{id}");
        var items = await ReadListAsync<EventItem>(response);
        return items.FirstOrDefault();
    }

    public async Task<EventItem> AddEventAsync(EventItem item)
    {
        var response = await SendAsync(HttpMethod.Post, "/rest/v1/events", item, returnRepresentation: true);
        var created = await ReadListAsync<EventItem>(response);
        return created.First();
    }

    public async Task UpdateEventAsync(EventItem item)
    {
        await SendAsync(HttpMethod.Patch, $"/rest/v1/events?id=eq.{item.Id}", item);
    }

    public async Task DeleteEventAsync(int id)
    {
        await SendAsync(HttpMethod.Delete, $"/rest/v1/events?id=eq.{id}");
    }

    // ---------------- Gallery ----------------

    public async Task<List<GalleryItem>> GetGalleryItemsAsync()
    {
        var response = await SendAsync(HttpMethod.Get, "/rest/v1/gallery_items?select=*");
        return await ReadListAsync<GalleryItem>(response);
    }

    public async Task<List<string>> GetEventNamesAsync()
    {
        var items = await GetGalleryItemsAsync();
        return items.Select(g => g.EventName).Distinct().OrderBy(n => n).ToList();
    }

    public async Task<GalleryItem> AddGalleryItemAsync(GalleryItem item)
    {
        var response = await SendAsync(HttpMethod.Post, "/rest/v1/gallery_items", item, returnRepresentation: true);
        var created = await ReadListAsync<GalleryItem>(response);
        return created.First();
    }

    public async Task UpdateGalleryItemAsync(GalleryItem item)
    {
        await SendAsync(HttpMethod.Patch, $"/rest/v1/gallery_items?id=eq.{item.Id}", item);
    }

    public async Task DeleteGalleryItemAsync(int id)
    {
        await SendAsync(HttpMethod.Delete, $"/rest/v1/gallery_items?id=eq.{id}");
    }

    // ---------------- Video link parsing (client-side only, no network hop) ----------------
    // Detects TikTok (renders via its embed widget) and YouTube (converted to an embeddable URL);
    // anything else is treated as an already-embeddable URL and dropped straight into an iframe.

    public static VideoLinkResult? ParseVideoLink(string url)
    {
        url = url.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var tikTokMatch = TikTokVideoIdPattern.Match(url);
        if (tikTokMatch.Success)
        {
            return new VideoLinkResult(url, tikTokMatch.Groups[1].Value);
        }

        var youTubeMatch = YouTubeIdPattern.Match(url);
        if (youTubeMatch.Success)
        {
            return new VideoLinkResult($"https://www.youtube.com/embed/{youTubeMatch.Groups[1].Value}", null);
        }

        return Uri.TryCreate(url, UriKind.Absolute, out _) ? new VideoLinkResult(url, null) : null;
    }

    public record VideoLinkResult(string EmbedUrl, string? TikTokVideoId);

    // ---------------- Storage ----------------

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var token = await _auth.GetAccessTokenAsync()
            ?? throw new InvalidOperationException("You must be logged in to upload files.");

        var extension = System.IO.Path.GetExtension(fileName);
        var safeExtension = Regex.IsMatch(extension, @"^\.[A-Za-z0-9]{1,5}$") ? extension.ToLowerInvariant() : "";
        var uniqueName = $"{Guid.NewGuid():N}{safeExtension}";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/storage/v1/object/gallary/{uniqueName}");
        request.Headers.Add("apikey", _publishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = content;

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Upload rejected ({(int)response.StatusCode}): {body}");
        }

        return $"{_baseUrl}/storage/v1/object/public/gallary/{uniqueName}";
    }

    // ---------------- Auth ----------------

    public async Task<bool> SignInAsync(string email, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/auth/v1/token?grant_type=password");
        request.Headers.Add("apikey", _publishableKey);
        request.Content = JsonContent.Create(new { email, password });

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var payload = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
        if (string.IsNullOrWhiteSpace(payload?.AccessToken))
        {
            return false;
        }

        await _auth.SetSessionAsync(payload.AccessToken, email);
        return true;
    }

    // ---------------- Internals ----------------

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, bool returnRepresentation = false)
    {
        using var request = new HttpRequestMessage(method, $"{_baseUrl}{path}");
        request.Headers.Add("apikey", _publishableKey);
        request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, NoStore = true };

        var token = await _auth.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token ?? _publishableKey);

        if (returnRepresentation)
        {
            request.Headers.Add("Prefer", "return=representation");
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static async Task<List<T>> ReadListAsync<T>(HttpResponseMessage response)
    {
        var items = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions);
        return items ?? new List<T>();
    }

    private class SupabaseAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
