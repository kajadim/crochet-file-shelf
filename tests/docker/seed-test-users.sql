-- Three verified accounts for the automated tests (password for all of them: Test1234!).
-- Registration needs an emailed code, so the accounts are created directly in the test database.
-- The third account is deleted by the account deletion test and created again here before every run.
-- The password hashes are ASP.NET Core Identity hashes of that password. Safe to run more than once.

INSERT INTO "Users" ("Id", "Email", "PasswordHash", "LastName", "CreatedAt", "AvatarUpdatedAt", "Bio", "FirstName", "Username") VALUES ('11111111-1111-1111-1111-111111111111', 'test.user1@example.com', 'AQAAAAIAAYagAAAAEGmJAleayAOHa9o/dcymFa/+Ug7ocqdvql1iZYru7k70zSWqdqFiT+77B8gNC+LJ8g==', 'One', '2026-01-01 00:00:00+00', NULL, NULL, 'Test', 'testuser1') ON CONFLICT DO NOTHING;
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "LastName", "CreatedAt", "AvatarUpdatedAt", "Bio", "FirstName", "Username") VALUES ('22222222-2222-2222-2222-222222222222', 'test.user2@example.com', 'AQAAAAIAAYagAAAAECoxXt0BFwLetIqm9ZLgVGOoQiqoinbAqP+cbIHwAYkXqrPOeHhRzPdpRsr7GJiUTg==', 'Two', '2026-01-01 00:00:00+00', NULL, NULL, 'Test', 'testuser2') ON CONFLICT DO NOTHING;
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "LastName", "CreatedAt", "AvatarUpdatedAt", "Bio", "FirstName", "Username") VALUES ('33333333-3333-3333-3333-333333333333', 'test.user3@example.com', 'AQAAAAIAAYagAAAAEGmJAleayAOHa9o/dcymFa/+Ug7ocqdvql1iZYru7k70zSWqdqFiT+77B8gNC+LJ8g==', 'Three', '2026-01-01 00:00:00+00', NULL, NULL, 'Test', 'testuser3') ON CONFLICT DO NOTHING;
