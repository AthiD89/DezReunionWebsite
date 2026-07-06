namespace DezReunionWebsite.Models;

public enum GalleryMediaType
{
    Photo,
    Video
}

public class GalleryItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string EventName { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? VideoUrl { get; set; }
    public string? TikTokVideoId { get; set; }
    public GalleryMediaType Type { get; set; } = GalleryMediaType.Photo;
}
