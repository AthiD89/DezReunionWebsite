-- Dez Reunion Events — Supabase setup script
-- Paste this whole file into Supabase's SQL Editor (Database > SQL Editor) and click "Run".
-- Safe to re-run: uses IF NOT EXISTS / ON CONFLICT where practical, but seed data will
-- duplicate if you run it twice against a non-empty table — only run the INSERTs once.

-- ============================== Tables ==============================

create table if not exists events (
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

create table if not exists gallery_items (
    id serial primary key,
    title text not null,
    event_name text not null,
    image_url text not null default '',
    video_url text,
    tik_tok_video_id text,
    type smallint not null default 0 -- 0 = Photo, 1 = Video
);

-- ============================== Row Level Security ==============================

alter table events enable row level security;
alter table gallery_items enable row level security;

drop policy if exists "Public read access" on events;
create policy "Public read access" on events
    for select using (true);

drop policy if exists "Authenticated write access" on events;
create policy "Authenticated write access" on events
    for all using (auth.role() = 'authenticated') with check (auth.role() = 'authenticated');

drop policy if exists "Public read access" on gallery_items;
create policy "Public read access" on gallery_items
    for select using (true);

drop policy if exists "Authenticated write access" on gallery_items;
create policy "Authenticated write access" on gallery_items
    for all using (auth.role() = 'authenticated') with check (auth.role() = 'authenticated');

-- Storage: run this after you've created the "gallery" bucket in the dashboard (Storage > New bucket, public on).
drop policy if exists "Public read gallery bucket" on storage.objects;
create policy "Public read gallery bucket" on storage.objects
    for select using (bucket_id = 'gallery');

drop policy if exists "Authenticated upload to gallery bucket" on storage.objects;
create policy "Authenticated upload to gallery bucket" on storage.objects
    for insert with check (bucket_id = 'gallery' and auth.role() = 'authenticated');

-- ============================== Seed data (run once) ==============================

insert into events (title, date, time, venue, address, dress_code, summary, description, image_url, ticket_url, price)
values
    (
        'Slow Queer Sundays',
        '2026-08-02',
        '12:00 - 21:00',
        'Pisto Lounge',
        'Gugulethu, Cape Town',
        'No dress code — come as you are',
        'Slow down after a big Saturday — good food, R&B & soul classics, and easy Sunday vibes for the 30+ crowd.',
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
        'The original Dez Reunion gathering at Pisto Lounge, Gugulethu — where it all began.',
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
