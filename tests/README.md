# Tests

Automated tests for Crochet File Shelf, written in Java (JUnit 5, Selenium) and run with Maven.
They test a bit of both sides of the application:

| Kind | Package | Tag | What it does | Needs a browser |
|---|---|---|---|---|
| API | `api` | `api` | Calls the backend REST API directly: sign-in, who may see or change a work, sharing, folders, palette, comments, validation | no |
| UI | `ui` | `ui` | Drives the Angular app in a real browser: sign-in, registration, creating and deleting works, palette, profile, theme, language | yes |
| Flow | `flows` | `flow` | Crosses both sides: data is prepared through the API, the result is checked in the browser | yes |

Page objects (`pages`) hide the CSS selectors from the tests, so a test reads like the user's steps.

## Running in Docker (nothing to install)

From the project root:

```bash
docker compose --profile test run --rm tests
```

This builds and starts a separate copy of the application (database, backend, frontend), creates two test accounts, starts a Chrome
in its own container and runs all the tests. The first run downloads the images and takes a few minutes; later runs take about two
minutes. Nothing has to be installed except Docker, and the accounts, settings and browser are all prepared.

- Watch the browser while the tests run: open <http://localhost:7900> (password: `secret`).
- Reports are written to `tests/target/surefire-reports`, screenshots of failed browser tests to `tests/screenshots`.
- Run only some tests: `docker compose --profile test run --rm tests mvn -B test -Dtest.groups=api`.
- After changing the tests, rebuild the image: `docker compose --profile test build tests`.
- Stop everything: `docker compose --profile app --profile test down`.

The test accounts (`test.user1@example.com`, `test.user2@example.com` and `test.user3@example.com`, password `Test1234!`) exist only in the separate
`crochetfileshelf_docker` database, so your own data is not touched.

## Running on your own machine

### Before running

1. **Database** — `docker compose up -d` in the project root.
2. **Backend** — start it (`dotnet run` in `backend`, or run it from Visual Studio).
3. **Frontend** — `npm start` in `frontend` (serves `http://localhost:4200` and proxies `/api` to the backend).
4. **Two test accounts.** Register two accounts in the application once (registration needs the emailed code, so it cannot be automated).
   The tests only create and delete their own uniquely named data, so the accounts stay clean.
5. **Settings.** Copy `src/test/resources/test.properties.example` to `src/test/resources/test.properties` and fill in the two accounts.

   | Setting | Meaning |
   |---|---|
   | `base.url` | Where the application is served (default `http://localhost:4200`) |
   | `browser` | `chrome` (default), `edge` or `firefox`. Selenium downloads the matching driver by itself |
   | `headless` | `true` runs the browser without a window |
   | `user1.*`, `user2.*` | Email, password and username of the two accounts |

   Every setting can also be given as an environment variable, e.g. `BROWSER=edge` or `USER1_EMAIL=...`.

   `user3.*` is optional: the account deletion test deletes that account, so it is skipped when the setting is empty (the Docker
   setup creates the account again before every run).

Requirements: JDK 17 or newer and Maven 3.9 or newer (or run the tests from IntelliJ IDEA, which bundles Maven).

### Running

```bash
cd tests
mvn test                          # everything
mvn test -Dtest.groups=api        # backend only, no browser needed
mvn test -Dtest.groups=ui         # browser tests only
mvn test -Dtest.groups=flow       # flows across both sides
mvn test -Dtest=WorkPermissionsApiTest        # one class
mvn test -Dtest=AuthUiTest#wrongPassword      # one test
```

The API tests take a few seconds, the browser tests about a minute.

## If a test fails

- A browser test saves a screenshot to `tests/screenshots/<Class>-<test>.png`.
- The tests leave no data behind in normal runs. If a run is stopped half way, leftover folders are named
  `E2E ...`, `Flow ...`, `Private ...` and so on, and can be deleted in the application.
- `test.properties not found` means step 5 was skipped.
- `Could not log in as ...` means the account in `test.properties` does not exist or the password is wrong.

## What is covered

- **Sign-in and registration**: correct and wrong data, guards for guests and signed-in users, username availability while typing, existing email, language choice.
- **Permissions**: a private work is invisible to others (404), a view-only member can only read (403 on changes), an editor can comment and resize the matrix but not rename, share or delete, removing a member or leaving takes access away, permission changes apply at once, invitation code rules.
- **Folders and palette**: nesting, duplicates, deleting with contents, moving works, ownership, color validation, search and sorting done by the server.
- **Comments**: dangerous HTML is removed, limits, only the author edits, an edit moves a comment to the top, notifications reach the other members but not the author.
- **Input validation** and **localized error messages**.
- **Flows**: a full work lifecycle in the browser, joining a shared work with a code, a notification link that opens the work, the Shared badge.
