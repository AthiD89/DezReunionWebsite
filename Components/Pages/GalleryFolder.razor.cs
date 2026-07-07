using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DezReunionWebsite.Models;
using DezReunionWebsite.Services;

namespace DezReunionWebsite.Components.Pages;

public partial class GalleryFolder
{
    [Parameter]
    public string EventName { get; set; } = "";

    private List<GalleryItem> _photos = new();
    private List<GalleryItem> _videos = new();
    private List<EventItem> _allEvents = new();
    private GalleryItem? _selectedItem;
    private string _moveTargetEvent = "";
    private string? _feedback;
    private string? _error;
    private bool _busy;
    private bool _needsTikTokReload;

    private List<EventItem> _otherEvents =>
        _allEvents.Where(e => e.Title != EventName).ToList();

    protected override async Task OnParametersSetAsync()
    {
        _selectedItem = null;
        _feedback = null;
        _error = null;
        try
        {
            _allEvents = await Supabase.GetAllEventsAsync();
            await ReloadItemsAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }

    private async Task ReloadItemsAsync()
    {
        var all = await Supabase.GetGalleryItemsAsync();
        var matching = all.Where(g => g.EventName == EventName).ToList();
        _photos = matching.Where(g => g.Type == GalleryMediaType.Photo).ToList();
        _videos = matching.Where(g => g.Type == GalleryMediaType.Video).ToList();
        _needsTikTokReload = _videos.Any(v => !string.IsNullOrWhiteSpace(v.TikTokVideoId));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_needsTikTokReload)
        {
            _needsTikTokReload = false;
            await JS.InvokeVoidAsync("reloadTikTokEmbeds");
        }
    }

    private void OpenItem(GalleryItem item)
    {
        _feedback = null;
        _error = null;
        _selectedItem = item;
        _moveTargetEvent = _otherEvents.FirstOrDefault()?.Title ?? "";
    }

    private void CloseItem() => _selectedItem = null;

    private async Task HandleDelete()
    {
        if (_selectedItem is null)
        {
            return;
        }

        var confirmed = await JS.InvokeAsync<bool>("confirm", $"Are you sure you want to delete \"{_selectedItem.Title}\"? This can't be undone.");
        if (!confirmed)
        {
            return;
        }

        var mediaLabel = _selectedItem.Type == GalleryMediaType.Photo ? "Photo" : "Video";
        _busy = true;
        _error = null;
        try
        {
            await Supabase.DeleteGalleryItemAsync(_selectedItem.Id);
            _selectedItem = null;
            _feedback = $"{mediaLabel} deleted.";
            await ReloadItemsAsync();
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

    private async Task HandleMove()
    {
        if (_selectedItem is null || string.IsNullOrWhiteSpace(_moveTargetEvent))
        {
            return;
        }

        var confirmed = await JS.InvokeAsync<bool>("confirm", $"Move \"{_selectedItem.Title}\" to \"{_moveTargetEvent}\"?");
        if (!confirmed)
        {
            return;
        }

        var mediaLabel = _selectedItem.Type == GalleryMediaType.Photo ? "Photo" : "Video";
        var destination = _moveTargetEvent;
        _busy = true;
        _error = null;
        try
        {
            _selectedItem.EventName = destination;
            await Supabase.UpdateGalleryItemAsync(_selectedItem);
            _selectedItem = null;
            _feedback = $"{mediaLabel} moved to \"{destination}\".";
            await ReloadItemsAsync();
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

    private static readonly string[] DirectVideoExtensions = { ".mp4", ".mov", ".webm", ".ogg", ".m4v" };

    private static bool IsDirectVideoFile(string? url) =>
        !string.IsNullOrWhiteSpace(url) &&
        DirectVideoExtensions.Any(ext => url.Contains(ext, StringComparison.OrdinalIgnoreCase));
}
