# Dez Reunion Events

The website for Dez Reunion Events — a Cape Town community events brand. Public pages for browsing events, RSVPing, and viewing photo/video galleries, plus a small admin area for managing events and gallery content.

Live architecture in one sentence: a **Blazor WebAssembly** app that runs entirely in the visitor's browser and talks directly to **Supabase** (Postgres + REST API + Auth + file storage) — there is no custom backend server.

## Tech stack

| Layer | What's used |
|---|---|
| UI framework | Blazor WebAssembly (.NET 10) |
| Database / API | [Supabase](https://supabase.com) — Postgres with auto-generated REST API (PostgREST) |
| Auth | Supabase Auth (email/password), used only for the admin area |
| File storage | Supabase Storage (bucket named `gallary`) for uploaded event/gallery photos and videos |
| Contact/RSVP email | [Web3Forms](https://web3forms.com) — forwards form submissions to the site owner's inbox |
| Ticket sales | [Quicket](https://www.quicket.co.za) (external) — see [How ticketing works](#how-ticketing-works) |
| Hosting | Netlify (static SPA hosting) |

There is no ASP.NET Core server, no custom API, and no database access code beyond a thin REST client — `Services/SupabaseClient.cs` calls Supabase's REST API directly from the browser using a public "publishable" API key. Write access (creating/editing/deleting events and gallery items) is gated by Supabase Row Level Security (RLS): anyone can *read*, only an *authenticated* admin session can *write*. See [supabase/setup.sql](supabase/setup.sql) for the exact policies.

## Project structure

```
Components/
  Layout/
    MainLayout.razor         Page chrome (header + footer + content)
    NavMenu.razor(.cs)        Top nav bar
    Footer.razor(.cs)         Site footer, incl. admin logout link
    WhatsAppButton.razor      Floating WhatsApp contact button
  Pages/
    Home.razor(.cs)           Landing page
    Events.razor(.cs)         Upcoming + past events list
    EventDetails.razor(.cs)   Single event page
    Gallery.razor(.cs)        Gallery "folders" (one per event)
    GalleryFolder.razor(.cs)  Photos/videos for one event
    Tickets.razor(.cs)        Ticket "Buy Now" (Quicket) + RSVP form
    Contact.razor(.cs)        Contact form (Web3Forms)
    About.razor               Static about page
    NotFound.razor            404 page
    Admin/
      Login.razor(.cs)          Admin sign-in
      Dashboard.razor(.cs)      Admin home: list/delete/edit events & gallery items
      EventForm.razor(.cs)      Create/edit an event
      GalleryForm.razor(.cs)    Create/edit a gallery photo/video
  Routes.razor                 Router + [Authorize] fallback UI
  _Imports.razor               Shared @using directives for all .razor files

Models/
  EventItem.cs                 Maps to the `events` table
  GalleryItem.cs                Maps to the `gallery_items` table
  RsvpRequest.cs                RSVP form model (also defines ContactMessage, the contact form model)

Services/
  SupabaseClient.cs             All Supabase REST calls (events, gallery, storage, auth) + TikTok/YouTube link parsing
  SupabaseAuthStateProvider.cs   Holds the admin session token in localStorage; wires into Blazor's [Authorize]
  FormEmailService.cs           Posts RSVP/contact submissions to Web3Forms

wwwroot/
  appsettings.json               Supabase URL/key + Web3Forms key (see Configuration below)
  index.html                     SPA shell; also loads TikTok's embed.js
  _headers, _redirects           Netlify: SPA fallback routing + no-cache rules for app-shell files
  images/                        Static SVG placeholders for events/gallery

supabase/setup.sql               One-shot script to (re)create the Supabase schema, RLS policies, and seed data
netlify.toml                     Netlify SPA redirect rule
```

### Why `.razor` + `.razor.cs` pairs?

Every component with logic is split into a markup file (`Foo.razor`) and a matching code-behind partial class (`Foo.razor.cs`). `@page`, `@attribute`, and `@inject` directives stay in the `.razor` file — they still generate members on the same partial class, so the `.razor.cs` file can use injected services (`Supabase`, `JS`, `Navigation`, etc.) without redeclaring them. The `.razor.cs` file needs its own explicit `using` statements since it doesn't inherit `_Imports.razor`.

## How things fit together

- **No server-side code runs for this app.** Every page loads the same static `index.html` + WASM payload; `SupabaseClient` makes REST calls straight from the visitor's browser to Supabase using the public `PublishableKey` in `appsettings.json`. That key is *meant* to be public — it can only do what Supabase's RLS policies allow (public read, authenticated write).
- **Admin auth** is Supabase's password-based auth. Logging in (`Login.razor`) gets a JWT access token, stored in `localStorage` by `SupabaseAuthStateProvider`. `SupabaseClient` attaches that token as a Bearer header on every request; if it's missing/expired, requests fall back to the public key (read-only in practice, since RLS blocks anonymous writes). `SupabaseAuthStateProvider` also decodes the JWT's `exp` claim client-side to detect an expired session and force a clean logout instead of letting write calls fail confusingly.
- **File uploads** (event photos, gallery photos/videos) go straight to Supabase Storage's `gallary` bucket via `SupabaseClient.UploadFileAsync`, using the admin's auth token.
- **Video embeds**: `SupabaseClient.ParseVideoLink` recognizes TikTok and YouTube URLs pasted into the gallery admin form and converts them into an embeddable form (TikTok video ID, or a YouTube `/embed/` URL). TikTok embeds render via TikTok's own `embed.js` widget (loaded in `index.html`); because that script only scans the page once on load, `GalleryFolder.razor.cs` re-triggers it after rendering any TikTok blockquote so it actually mounts (Blazor being a SPA, the blockquote often appears after the initial scan already ran). Links TikTok can't resolve to a numeric video ID (e.g. unresolved `vt.tiktok.com` short links) fall back to a plain "Watch on TikTok" button instead of a broken iframe — TikTok blocks iframe-embedding entirely outside its own widget.
- **Emails**: Web3Forms is the only email integration. It notifies the *site owner* of new RSVP/contact submissions — it cannot email the guest back. Don't assume otherwise (see below).

## How ticketing works

Real paid tickets are sold on **Quicket**, not on this site. An event's `TicketUrl` field (set in the admin Event form) is a Quicket link; when set, the public Events/Tickets pages show a "Buy Now" button that sends visitors to Quicket, where they pay and Quicket emails them their own QR-code ticket.

The **RSVP form** on the Tickets page is a separate, simpler thing: it's for free or pay-at-the-door events, and it just forwards the guest's details to the site owner via Web3Forms so they can follow up personally. It is *not* a ticket and does not generate one — the copy on that page is intentionally worded to make that distinction clear. Don't build ticket/QR-email generation into this app; if a real ticketed flow is ever needed for free events too, that's a Quicket-side decision, not something to bolt onto the RSVP form.

## Configuration

`wwwroot/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "https://<project-ref>.supabase.co",
    "PublishableKey": "sb_publishable_..."
  },
  "Web3Forms": {
    "AccessKey": "..."
  }
}
```

Both keys are intentionally public/client-safe:
- The Supabase key is the "publishable"/anon key — safe to ship in a WASM bundle because RLS policies (in `supabase/setup.sql`) restrict what it can actually do.
- The Web3Forms access key only allows *sending* a submission to the configured owner inbox, not reading anything back.

## Running locally

```
dotnet run
```

Serves at `http://localhost:5096` (or `https://localhost:7009`) per `Properties/launchSettings.json`. No local database or server setup needed — by default it talks to the live production Supabase project via the keys already committed in `appsettings.json`.

## Supabase setup

The production project's URL and key are already in `wwwroot/appsettings.json` — you only need this section to point the app at a **different** Supabase project (a personal dev/test project, or to rebuild production from scratch in a disaster-recovery scenario).

1. Create a project at [supabase.com](https://supabase.com).
2. Open **SQL Editor**, paste the entire contents of [`supabase/setup.sql`](supabase/setup.sql), and run it. This creates the `events` and `gallery_items` tables, enables Row Level Security, adds public-read/authenticated-write policies, and seeds a few sample events and gallery photos.
3. Go to **Storage** and create a new bucket named exactly `gallary` (yes, misspelled on purpose — see [Known gotchas](#known-gotchas)), set to **public**. The storage policy statements near the bottom of `setup.sql` reference `storage.objects` and only take effect once this bucket exists, so re-run those specific statements afterward if you ran the whole script before creating the bucket.
4. Go to **Authentication → Users** and manually add one user (email + password) — this becomes the admin account that can log in at `/admin/login`. There's no self-service sign-up in this app by design; only someone with access to the Supabase dashboard can create new admin logins.
5. Go to **Project Settings → API** and copy the **Project URL** and the **anon/publishable key**.
6. Paste those into `wwwroot/appsettings.json`:
   ```json
   {
     "Supabase": {
       "Url": "https://<project-ref>.supabase.co",
       "PublishableKey": "sb_publishable_..."
     }
   }
   ```

**Do not re-run `setup.sql` against the live production project** once it has real data in it — the script starts with `drop table if exists ... cascade`, so it deletes both tables before recreating them from scratch.

## Deployment

Every push to `main` triggers [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml), which does the actual build and deploy — Netlify itself never builds this project:

1. Checks out the repo and installs .NET 10.
2. Runs `dotnet publish DezReunionWebsite.csproj -c Release -o publish_output`.
3. Installs the Netlify CLI and runs `netlify deploy --prod --no-build --dir=publish_output/wwwroot`, pushing the published static files straight to the Netlify site.

That last step needs two repository secrets (**Settings → Secrets and variables → Actions**):
- `NETLIFY_AUTH_TOKEN` — a Netlify personal access token
- `NETLIFY_SITE_ID` — the target Netlify site's ID

Netlify-side config that's committed to the repo (used at request-serving time, not build time):
- `netlify.toml` / `wwwroot/_redirects` — rewrite every path to `/index.html` so client-side routing works on a hard refresh or a direct link to e.g. `/events/3`.
- `wwwroot/_headers` — disables caching on `index.html`, `blazor.boot.json`, and `appsettings.json` so a new deploy is picked up immediately instead of visitors getting a stale cached app shell.

To deploy manually without waiting for GitHub Actions, run the same three commands locally (with the Netlify CLI installed and `netlify login`/`netlify link` done once). There's also a `.netlify/` folder the Netlify CLI creates locally when you run `netlify link` — it's gitignored and has no effect on the actual deploy pipeline above.

## Known gotchas

- **Admin writes rely entirely on Supabase RLS**, not just the app's `[Authorize]` attribute (which is UI-only). If you ever change the schema, double-check the RLS policies in `supabase/setup.sql` still match — a misconfigured policy would let anyone write directly via the REST API regardless of what the Blazor UI shows.
- **Gallery titles**: photo/video titles are auto-filled from the uploaded file's name (`GalleryForm.razor.cs`'s `FileNameToTitle`), with a junk-filename detector (GUIDs, camera-roll temp names, CDN hashes) that falls back to a generic "Photo"/"Video" label. It's a heuristic, not perfect — admins should still glance at auto-filled titles before saving.
- **The `gallary` storage bucket is intentionally misspelled** to match one already created in the live Supabase project — don't "fix" the typo without also renaming the actual bucket and updating every reference.
