using DezReunionWebsite.Models;

namespace DezReunionWebsite.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Events.Any())
        {
            return;
        }

        var events = new List<EventItem>
        {
            new EventItem
            {
                Title = "Slow Queer Sundays",
                Date = new DateTime(2026, 8, 2),
                Time = "12:00 - 21:00",
                Venue = "Pisto Lounge",
                Address = "Gugulethu, Cape Town",
                DressCode = "No dress code — come as you are",
                Summary = "Slow down after a big Saturday — good food, R&B & soul classics, and easy Sunday vibes for the 30+ crowd.",
                Description = "Saturday was loud. Sunday doesn't have to be. Whether you danced until sunrise, worked all weekend, or just need to recharge, Slow Queer Sundays is your place to slow down. Come grab a delicious plate of food, sip something cold, sink into the music, and spend the afternoon with good people. No pressure. No dress code. No club energy. No under 30s. Just timeless R&B, soul, conversation, laughter and a community that feels like home. Good food, slow jam classics, a mature crowd (30+), beautiful Sunday vibes, and a safe queer space. Come for lunch. Stay until sunset.",
                ImageUrl = "/images/events/slow-queer-sundays.svg",
                TicketUrl = null,
                Price = null
            },
            new EventItem
            {
                Title = "Games Night After Hours Affair",
                Date = new DateTime(2026, 5, 30),
                Venue = "The Concept Bar",
                Address = "Long Street, Cape Town",
                Summary = "An after-hours games night affair at The Concept Bar on Long Street.",
                Description = "Games Night After Hours Affair kept the community together for a fun evening of games late into the night at The Concept Bar, Long Street, Cape Town.",
                ImageUrl = "/images/events/games-night-after-hours.svg"
            },
            new EventItem
            {
                Title = "Meet & Mingle Games Night",
                Date = new DateTime(2026, 4, 4),
                Venue = "The Sky Lounge",
                Address = "Long Street, Cape Town",
                Summary = "A relaxed games night mixer at The Sky Lounge on Long Street.",
                Description = "Meet & Mingle Games Night brought the community together for an evening of games and mingling at The Sky Lounge, Long Street, Cape Town.",
                ImageUrl = "/images/events/meet-mingle-games-night.svg"
            },
            new EventItem
            {
                Title = "Dez Reunion",
                Date = new DateTime(2026, 2, 28),
                Venue = "Pisto Lounge",
                Address = "Gugulethu, Cape Town",
                Summary = "The original Dez Reunion gathering at Pisto Lounge, Gugulethu — where it all began.",
                Description = "The very first Dez Reunion event, hosted at Pisto Lounge in Gugulethu, brought our community together and started it all.",
                ImageUrl = "/images/events/dez-reunion-original.svg"
            }
        };

        db.Events.AddRange(events);

        db.GalleryItems.AddRange(
            new GalleryItem { Title = "Pisto Lounge crowd", EventName = "Dez Reunion", ImageUrl = "/images/gallery/gallery-1.svg" },
            new GalleryItem { Title = "First reunion toast", EventName = "Dez Reunion", ImageUrl = "/images/gallery/gallery-2.svg" },
            new GalleryItem { Title = "Games night table", EventName = "Meet & Mingle Games Night", ImageUrl = "/images/gallery/gallery-3.svg" },
            new GalleryItem { Title = "Sky Lounge mingling", EventName = "Meet & Mingle Games Night", ImageUrl = "/images/gallery/gallery-4.svg" },
            new GalleryItem { Title = "After hours at Concept Bar", EventName = "Games Night After Hours Affair", ImageUrl = "/images/gallery/gallery-5.svg" },
            new GalleryItem { Title = "Late night games", EventName = "Games Night After Hours Affair", ImageUrl = "/images/gallery/gallery-6.svg" },
            new GalleryItem
            {
                Title = "Dez Reunion Highlights",
                EventName = "Dez Reunion",
                ImageUrl = "/images/gallery/gallery-2.svg",
                VideoUrl = "https://www.tiktok.com/@dez.reunion/video/7658989575179603221",
                TikTokVideoId = "7658989575179603221",
                Type = GalleryMediaType.Video
            },
            new GalleryItem
            {
                Title = "Dez Reunion Highlights 2",
                EventName = "Dez Reunion",
                ImageUrl = "/images/gallery/gallery-1.svg",
                VideoUrl = "https://www.tiktok.com/@dez.reunion/video/7629368817348578581",
                TikTokVideoId = "7629368817348578581",
                Type = GalleryMediaType.Video
            }
        );

        db.SaveChanges();
    }
}
