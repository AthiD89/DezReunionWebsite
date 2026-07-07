using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace DezReunionWebsite.Services;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "dezreunion_access_token";
    private const string EmailKey = "dezreunion_admin_email";

    private readonly IJSRuntime _js;
    private string? _cachedToken;

    public SupabaseAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (IsExpired(token))
        {
            await LogOutAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var email = await _js.InvokeAsync<string?>("localStorage.getItem", EmailKey);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email ?? "admin") }, "supabase");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        _cachedToken ??= await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        return _cachedToken;
    }

    public async Task SetSessionAsync(string accessToken, string email)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", EmailKey, email);
        _cachedToken = accessToken;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogOutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", EmailKey);
        _cachedToken = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static bool IsExpired(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return true;
            }

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);
            if (!doc.RootElement.TryGetProperty("exp", out var expElement))
            {
                return false;
            }

            var expiry = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
            return expiry <= DateTimeOffset.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
