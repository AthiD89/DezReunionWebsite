using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages;

public partial class Home
{
    private EventItem? _nextEvent;
    private List<GalleryItem> _previewGallery = new();

    protected override async Task OnInitializedAsync()
    {
        var upcoming = await Supabase.GetUpcomingEventsAsync();
        _nextEvent = upcoming.FirstOrDefault();

        var gallery = await Supabase.GetGalleryItemsAsync();
        _previewGallery = gallery
            .Where(g => g.Type == GalleryMediaType.Photo)
            .Take(6)
            .ToList();
    }
}
