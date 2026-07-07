namespace DezReunionWebsite.Components.Pages;

public partial class Gallery
{
    private List<FolderSummary> _folders = new();

    protected override async Task OnInitializedAsync()
    {
        var items = await Supabase.GetGalleryItemsAsync();

        _folders = items
            .GroupBy(g => g.EventName)
            .Select(group => new FolderSummary(
                group.Key,
                group.OrderBy(g => g.Type).First().ImageUrl,
                group.Count()))
            .OrderBy(f => f.EventName)
            .ToList();
    }

    private record FolderSummary(string EventName, string CoverImageUrl, int Count);
}
