-- Dez Reunion Events - Supabase setup script
-- Paste this whole file into Supabase's SQL Editor (Database > SQL Editor) and click "Run".
-- Starts by dropping both tables so it always rebuilds from a clean slate, regardless of
-- any partial/broken state left over from a previous attempt. Safe to run multiple times
-- ONLY during initial setup. Once the site is live and you've added real events/gallery
-- items through the admin area, do NOT run this again - it will delete everything.

-- ============================== Clean slate ==============================

drop table if exists gallery_items cascade;
drop table if exists events cascade;

-- ============================== Tables ==============================

create table events (
    id serial primary key,
    title text not null,
    date date not null,
    time text,
    venue text not null,
    address text not null,
    dress_code text,
    summary text not null,
    description text not null,
    image_url text not null default '',
    ticket_url text,
    price numeric(10, 2)
);

create table gallery_items (
    id serial primary key,
    title text not null,
    event_name text not null,
    image_url text not null default '',
    video_url text,
    tik_tok_video_id text,
    type smallint not null default 0
);

-- ============================== Row Level Security ==============================

alter table events enable row level security;
alter table gallery_items enable row level security;

drop policy if exists events_public_read on events;
create policy events_public_read on events
    for select using (true);

drop policy if exists events_authenticated_write on events;
create policy events_authenticated_write on events
    for all using (auth.uid() is not null) with check (auth.uid() is not null);

drop policy if exists gallery_items_public_read on gallery_items;
create policy gallery_items_public_read on gallery_items
    for select using (true);

drop policy if exists gallery_items_authenticated_write on gallery_items;
create policy gallery_items_authenticated_write on gallery_items
    for all using (auth.uid() is not null) with check (auth.uid() is not null);

-- Storage: run this after you've created the "gallery" bucket in the dashboard (Storage > New bucket, public on).
drop policy if exists gallery_bucket_public_read on storage.objects;
create policy gallery_bucket_public_read on storage.objects
    for select using (bucket_id = 'gallery');

drop policy if exists gallery_bucket_authenticated_upload on storage.objects;
create policy gallery_bucket_authenticated_upload on storage.objects
    for insert with check (bucket_id = 'gallery' and auth.uid() is not null);

-- ============================== Seed data ==============================
-- Since the tables get dropped and recreated above, running this whole script again
-- later is fine too - it won't create duplicates (it just starts fresh each time).
-- Only worry about duplicates if you copy just this INSERT section on its own.

insert into events (title, date, time, venue, address, dress_code, summary, description, image_url, ticket_url, price)
values
    (
        'Slow Queer Sundays',
        '2026-08-02',
        '12:00 - 21:00',
        'Pisto Lounge',
        'Gugulethu, Cape Town',
        'No dress code - come as you are',
        'Slow down after a big Saturday - good food, R&B & soul classics, and easy Sunday vibes for the 30+ crowd.',
        'Saturday was loud. Sunday doesn''t have to be. Whether you danced until sunrise, worked all weekend, or just need to recharge, Slow Queer Sundays is your place to slow down. Come grab a delicious plate of food, sip something cold, sink into the music, and spend the afternoon with good people. No pressure. No dress code. No club energy. No under 30s. Just timeless R&B, soul, conversation, laughter and a community that feels like home. Good food, slow jam classics, a mature crowd (30+), beautiful Sunday vibes, and a safe queer space. Come for lunch. Stay until sunset.',
        '/images/events/slow-queer-sundays.svg',
        null,
        null
    ),
    (
        'Games Night After Hours Affair',
        '2026-05-30',
        null,
        'The Concept Bar',
        'Long Street, Cape Town',
        null,
        'An after-hours games night affair at The Concept Bar on Long Street.',
        'Games Night After Hours Affair kept the community together for a fun evening of games late into the night at The Concept Bar, Long Street, Cape Town.',
        '/images/events/games-night-after-hours.svg',
        null,
        null
    ),
    (
        'Meet & Mingle Games Night',
        '2026-04-04',
        null,
        'The Sky Lounge',
        'Long Street, Cape Town',
        null,
        'A relaxed games night mixer at The Sky Lounge on Long Street.',
        'Meet & Mingle Games Night brought the community together for an evening of games and mingling at The Sky Lounge, Long Street, Cape Town.',
        '/images/events/meet-mingle-games-night.svg',
        null,
        null
    ),
    (
        'Dez Reunion',
        '2026-02-28',
        null,
        'Pisto Lounge',
        'Gugulethu, Cape Town',
        null,
        'The original Dez Reunion gathering at Pisto Lounge, Gugulethu - where it all began.',
        'The very first Dez Reunion event, hosted at Pisto Lounge in Gugulethu, brought our community together and started it all.',
        '/images/events/dez-reunion-original.svg',
        null,
        null
    );

insert into gallery_items (title, event_name, image_url, video_url, tik_tok_video_id, type)
values
    ('Pisto Lounge crowd', 'Dez Reunion', '/images/gallery/gallery-1.svg', null, null, 0),
    ('First reunion toast', 'Dez Reunion', '/images/gallery/gallery-2.svg', null, null, 0),
    ('Games night table', 'Meet & Mingle Games Night', '/images/gallery/gallery-3.svg', null, null, 0),
    ('Sky Lounge mingling', 'Meet & Mingle Games Night', '/images/gallery/gallery-4.svg', null, null, 0),
    ('After hours at Concept Bar', 'Games Night After Hours Affair', '/images/gallery/gallery-5.svg', null, null, 0),
    ('Late night games', 'Games Night After Hours Affair', '/images/gallery/gallery-6.svg', null, null, 0),
    (
        'Dez Reunion Highlights',
        'Dez Reunion',
        '/images/gallery/gallery-2.svg',
        'https://www.tiktok.com/@dez.reunion/video/7658989575179603221',
        '7658989575179603221',
        1
    ),
    (
        'Dez Reunion Highlights 2',
        'Dez Reunion',
        '/images/gallery/gallery-1.svg',
        'https://www.tiktok.com/@dez.reunion/video/7629368817348578581',
        '7629368817348578581',
        1
    );
