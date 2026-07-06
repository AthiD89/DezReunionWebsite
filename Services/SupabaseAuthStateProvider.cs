using System.Security.Claims;
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
}
