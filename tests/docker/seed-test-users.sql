-- Two verified accounts for the automated tests (password for both: Test1234!).
-- Registration needs an emailed code, so the accounts are created directly in the test database.
-- The password hashes are ASP.NET Core Identity hashes of that password. Safe to run more than once.

INSERT INTO "Users" ("Id", "Email", "PasswordHash", "LastName", "CreatedAt", "AvatarUpdatedAt", "Bio", "FirstName", "Username") VALUES ('11111111-1111-1111-1111-111111111111', 'test.user1@example.com', 'AQAAAAIAAYagAAAAEGmJAleayAOHa9o/dcymFa/+Ug7ocqdvql1iZYru7k70zSWqdqFiT+77B8gNC+LJ8g==', 'One', '2026-01-01 00:00:00+00', NULL, NULL, 'Test', 'testuser1') ON CONFLICT DO NOTHING;
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "LastName", "CreatedAt", "AvatarUpdatedAt", "Bio", "FirstName", "Username") VALUES ('22222222-2222-2222-2222-222222222222', 'test.user2@example.com', 'AQAAAAIAAYagAAAAECoxXt0BFwLetIqm9ZLgVGOoQiqoinbAqP+cbIHwAYkXqrPOeHhRzPdpRsr7GJiUTg==', 'Two', '2026-01-01 00:00:00+00', NULL, NULL, 'Test', 'testuser2') ON CONFLICT DO NOTHING;
