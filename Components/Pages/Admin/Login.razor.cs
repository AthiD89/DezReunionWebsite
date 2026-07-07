namespace DezReunionWebsite.Components.Pages.Admin;

public partial class Login
{
    private LoginModel _model = new();
    private string? _error;
    private bool _signingIn;

    private async Task HandleLogin()
    {
        _error = null;
        _signingIn = true;
        try
        {
            var success = await Supabase.SignInAsync(_model.Email, _model.Password);
            if (!success)
            {
                _error = "Invalid email or password.";
                return;
            }

            Navigation.NavigateTo("/admin", forceLoad: true);
        }
        catch (Exception)
        {
            _error = "Couldn't reach the server. Check your connection and try again.";
        }
        finally
        {
            _signingIn = false;
        }
    }

    private class LoginModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
