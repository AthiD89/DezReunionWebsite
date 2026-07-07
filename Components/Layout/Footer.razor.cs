using DezReunionWebsite.Services;

namespace DezReunionWebsite.Components.Layout;

public partial class Footer
{
    private async Task HandleLogOut()
    {
        await AuthState.LogOutAsync();
        Navigation.NavigateTo("/");
    }
}
