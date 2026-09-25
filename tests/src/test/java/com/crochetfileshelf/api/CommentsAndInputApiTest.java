package com.crochetfileshelf.api;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: comments and input validation")
public class CommentsAndInputApiTest extends ApiTestBase {

    @Test
    @DisplayName("Dangerous HTML in a comment is removed before it is stored")
    void commentsAreSanitized() {
        String folderId = createFolder(alice, unique("Xss"), null);
        String workId = createSiteWork(alice, folderId, "Comment target");
        String path = "/api/works/" + workId + "/comments";

        String malicious = "<p onclick=\"steal()\">Hello <strong>world</strong></p>"
                + "<script>alert(1)</script>"
                + "<img src=x onerror=\"alert(2)\">"
                + "<a href=\"javascript:alert(3)\">click</a>";

        ApiClient.Response created = alice.post(path, body("text", malicious));
        assertTrue(created.isOk(), created.raw());

        String stored = created.json().get("text").asText().toLowerCase();
        assertFalse(stored.contains("<script"), stored);
        assertFalse(stored.contains("onerror"), stored);
        assertFalse(stored.contains("onclick"), stored);
        assertFalse(stored.contains("javascript:"), stored);
        assertTrue(stored.contains("<strong>world</strong>"), "harmless formatting is kept: " + stored);
    }

    @Test
    @DisplayName("Empty and oversized comments are rejected")
    void commentLimits() {
        String folderId = createFolder(alice, unique("Limits"), null);
        String workId = createSiteWork(alice, folderId, "Limit target");
        String path = "/api/works/" + workId + "/comments";

        assertEquals(400, alice.post(path, body("text", "<p>   </p>")).status());
        assertEquals(400, alice.post(path, body("text", "")).status());
        assertEquals(400, alice.post(path, body("text", "<p>" + "a".repeat(20_000) + "</p>")).status());
    }

    @Test
    @DisplayName("Only the author edits or deletes a comment")
    void onlyAuthorChangesComment() {
        String folderId = createFolder(alice, unique("Authors"), null);
        String workId = createSiteWork(alice, folderId, "Discussed");
        share(alice, bob, workId, "CanEdit");
        String path = "/api/works/" + workId + "/comments";

        String commentId = bob.post(path, body("text", "<p>Bob's comment</p>")).id();

        assertEquals(403, alice.put(path + "/" + commentId, body("text", "<p>edited by owner</p>")).status());
        assertTrue(bob.put(path + "/" + commentId, body("text", "<p>edited by author</p>")).isOk());
        assertTrue(bob.delete(path + "/" + commentId).isOk());
    }

    @Test
    @DisplayName("Comments are listed by the newest activity first, and an edit moves a comment to the top")
    void editedCommentMovesToTop() throws InterruptedException {
        String folderId = createFolder(alice, unique("Order"), null);
        String workId = createSiteWork(alice, folderId, "Ordered");
        String path = "/api/works/" + workId + "/comments";

        String first = alice.post(path, body("text", "<p>first</p>")).id();
        Thread.sleep(30);
        alice.post(path, body("text", "<p>second</p>"));
        Thread.sleep(30);

        assertTrue(alice.get(path).json().get(0).get("text").asText().contains("second"));

        alice.put(path + "/" + first, body("text", "<p>first, edited</p>"));

        assertTrue(alice.get(path).json().get(0).get("text").asText().contains("edited"));
    }

    @Test
    @DisplayName("Members get a notification when someone comments, the author does not")
    void commentNotifiesOthers() {
        String folderId = createFolder(alice, unique("Notify"), null);
        String workName = unique("Notified");
        String workId = createSiteWork(alice, folderId, workName);
        share(alice, bob, workId, "CanEdit");

        int before = notificationsAbout(alice, workName);
        int bobBefore = notificationsAbout(bob, workName);

        assertTrue(bob.post("/api/works/" + workId + "/comments", body("text", "<p>ping</p>")).isOk());

        assertEquals(before + 1, notificationsAbout(alice, workName), "the owner is notified");
        assertEquals(bobBefore, notificationsAbout(bob, workName), "the author is not notified about their own comment");
    }

    @Test
    @DisplayName("Work input is validated on the server")
    void workInputIsValidated() {
        String folderId = createFolder(alice, unique("Validate"), null);

        assertEquals(400, alice.post("/api/works", body("name", "", "type", "Site", "folderId", folderId, "url", "example.com")).status());
        assertEquals(400, alice.post("/api/works", body("name", "No type", "folderId", folderId)).status());
        assertEquals(400, alice.post("/api/works", body("name", "No folder", "type", "Site", "url", "example.com")).status());
        assertEquals(400, alice.post("/api/works", body("name", "No link", "type", "Site", "folderId", folderId)).status());
        assertEquals(400, alice.post("/api/works", body("name", "Half size", "type", "Pattern", "folderId", folderId, "width", 10)).status());
        assertEquals(400, alice.post("/api/works", body("name", "Huge", "type", "Pattern", "folderId", folderId, "width", 5000, "height", 5000)).status());
        assertEquals(400, alice.post("/api/works", body("name", "Bad video", "type", "Video", "folderId", folderId, "url", "https://example.com/not-a-video")).status());
    }

    @Test
    @DisplayName("Matrix: positions outside the grid are refused and cells survive a reload")
    void matrixBasics() {
        String folderId = createFolder(alice, unique("Matrix"), null);
        String workId = createPatternWork(alice, folderId, "Grid");
        String colorId = createColor(alice, unique("Paint"), "#AA5500");
        String path = "/api/works/" + workId + "/pattern";

        assertEquals(400, alice.put(path + "/position", body("row", 99, "column", 0)).status());
        assertTrue(alice.put(path + "/position", body("row", 2, "column", 3)).isOk());

        assertTrue(alice.put(path + "/cells", body("cells", java.util.List.of(
                body("row", 1, "column", 1, "colorId", colorId)))).isOk());

        ApiClient.Response pattern = alice.get(path);
        assertEquals(2, pattern.json().get("currentRow").asInt());
        assertEquals(3, pattern.json().get("currentColumn").asInt());
        assertEquals(1, pattern.json().get("cells").size());
        assertEquals("#AA5500", pattern.json().get("cells").get(0).get("hexValue").asText().toUpperCase());
    }

    private int notificationsAbout(ApiClient user, String workName) {
        String raw = user.get("/api/notifications").raw();
        int count = 0;
        int index = 0;
        while ((index = raw.indexOf(workName, index)) >= 0) {
            count++;
            index += workName.length();
        }
        return count;
    }
}
