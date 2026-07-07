using Microsoft.JSInterop;
using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages.Admin;

public partial class Dashboard
{
    private List<EventItem> _events = new();
    private List<GalleryItem> _gallery = new();
    private string? _error;
    private bool _busy;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _events = await Supabase.GetAllEventsAsync();
            _gallery = await Supabase.GetGalleryItemsAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }

    private async Task DeleteEvent(int id, string title)
    {
        var confirmed = await JS.InvokeAsync<bool>("confirm", $"Are you sure you want to delete \"{title}\"? This can't be undone.");
        if (!confirmed)
        {
            return;
        }

        _error = null;
        _busy = true;
        try
        {
            await Supabase.DeleteEventAsync(id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task DeleteGalleryItem(int id, string title)
    {
        var label = string.IsNullOrWhiteSpace(title) ? "this item" : $"\"{title}\"";
        var confirmed = await JS.InvokeAsync<bool>("confirm", $"Are you sure you want to delete {label}? This can't be undone.");
        if (!confirmed)
        {
            return;
        }

        _error = null;
        _busy = true;
        try
        {
            await Supabase.DeleteGalleryItemAsync(id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _busy = false;
        }
    }
}
