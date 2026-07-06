namespace DezReunionWebsite.Services;

public class SupabaseStorageService
{
    private const string BucketName = "gallery";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["SupabaseUrl"]) &&
        !string.IsNullOrWhiteSpace(_configuration["SupabaseServiceKey"]);

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        var supabaseUrl = _configuration["SupabaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("SupabaseUrl is not configured.");
        var serviceKey = _configuration["SupabaseServiceKey"]
            ?? throw new InvalidOperationException("SupabaseServiceKey is not configured.");

        var uniqueName = $"{Guid.NewGuid():N}-{fileName}";
        var uploadUrl = $"{supabaseUrl}/storage/v1/object/{BucketName}/{uniqueName}";

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", serviceKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return $"{supabaseUrl}/storage/v1/object/public/{BucketName}/{uniqueName}";
    }
}
