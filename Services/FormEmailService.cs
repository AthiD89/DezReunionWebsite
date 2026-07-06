using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace DezReunionWebsite.Services;

public class FormEmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public FormEmailService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public async Task<bool> SendAsync(string subject, Dictionary<string, string> fields)
    {
        var accessKey = _configuration["Web3Forms:AccessKey"];
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new InvalidOperationException("Web3Forms access key is not configured.");
        }

        var payload = new Dictionary<string, string>(fields)
        {
            ["access_key"] = accessKey,
            ["subject"] = subject
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.web3forms.com/submit");
        request.Headers.Add("Accept", "application/json");
        request.Content = JsonContent.Create(payload);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<Web3FormsResponse>();
        return result?.Success ?? false;
    }

    private class Web3FormsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
