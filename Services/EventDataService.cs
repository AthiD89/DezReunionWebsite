using DezReunionWebsite.Data;
using DezReunionWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace DezReunionWebsite.Services;

public class EventDataService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public EventDataService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public IReadOnlyList<EventItem> GetAllEvents()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Events.AsNoTracking().OrderBy(e => e.Date).ToList();
    }

    public IReadOnlyList<EventItem> GetUpcomingEvents()
    {
        using var db = _dbFactory.CreateDbContext();
        var today = DateTime.Today;
        return db.Events.AsNoTracking()
            .Where(e => e.Date.Date >= today)
            .OrderBy(e => e.Date)
            .ToList();
    }

    public IReadOnlyList<EventItem> GetPastEvents()
    {
        using var db = _dbFactory.CreateDbContext();
        var today = DateTime.Today;
        return db.Events.AsNoTracking()
            .Where(e => e.Date.Date < today)
            .OrderByDescending(e => e.Date)
            .ToList();
    }

    public EventItem? GetEventById(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Events.AsNoTracking().FirstOrDefault(e => e.Id == id);
    }

    public IReadOnlyList<GalleryItem> GetGalleryItems()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.GalleryItems.AsNoTracking().ToList();
    }

    public IReadOnlyList<string> GetEventNames()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.GalleryItems.AsNoTracking()
            .Select(g => g.EventName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    public int AddEvent(EventItem item)
    {
        using var db = _dbFactory.CreateDbContext();
        db.Events.Add(item);
        db.SaveChanges();
        return item.Id;
    }

    public void UpdateEvent(EventItem item)
    {
        using var db = _dbFactory.CreateDbContext();
        db.Events.Update(item);
        db.SaveChanges();
    }

    public void DeleteEvent(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        var existing = db.Events.FirstOrDefault(e => e.Id == id);
        if (existing is not null)
        {
            db.Events.Remove(existing);
            db.SaveChanges();
        }
    }

    public int AddGalleryItem(GalleryItem item)
    {
        using var db = _dbFactory.CreateDbContext();
        db.GalleryItems.Add(item);
        db.SaveChanges();
        return item.Id;
    }

    public void DeleteGalleryItem(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        var existing = db.GalleryItems.FirstOrDefault(g => g.Id == id);
        if (existing is not null)
        {
            db.GalleryItems.Remove(existing);
            db.SaveChanges();
        }
    }
}
