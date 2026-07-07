namespace DezReunionWebsite.Components.Layout;

public partial class NavMenu
{
    private bool _isOpen;

    private void ToggleMenu() => _isOpen = !_isOpen;
    private void CloseMenu() => _isOpen = false;
}
