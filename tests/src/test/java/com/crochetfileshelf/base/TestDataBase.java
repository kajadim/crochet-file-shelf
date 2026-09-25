package com.crochetfileshelf.base;

import com.crochetfileshelf.api.ApiClient;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;

import java.util.ArrayDeque;
import java.util.Deque;
import java.util.concurrent.ThreadLocalRandom;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.assertTrue;

public abstract class TestDataBase {

    protected ApiClient alice;
    protected ApiClient bob;

    private final Deque<Runnable> cleanup = new ArrayDeque<>();

    @BeforeEach
    protected void logInBothUsers() {
        alice = ApiClient.loginAs(TestConfig.USER1_EMAIL, TestConfig.USER1_PASSWORD);
        bob = ApiClient.loginAs(TestConfig.USER2_EMAIL, TestConfig.USER2_PASSWORD);
    }

    @AfterEach
    protected void removeCreatedData() {
        while (!cleanup.isEmpty()) {
            try {
                cleanup.pop().run();
            } catch (RuntimeException ignored) {
                // best effort: a test may already have deleted the item itself
            }
        }
    }

    protected static String unique(String prefix) {
        return prefix + " " + System.currentTimeMillis() % 1_000_000 + "-" + ThreadLocalRandom.current().nextInt(100, 999);
    }

    protected String createFolder(ApiClient owner, String name, String parentId) {
        ApiClient.Response response = owner.post("/api/folders", body("name", name, "parentFolderId", parentId));
        assertTrue(response.isOk(), "creating folder failed: " + response.raw());
        String id = response.id();
        if (parentId == null) {
            cleanup.push(() -> owner.delete("/api/folders/" + id));
        }
        return id;
    }

    protected String createSiteWork(ApiClient owner, String folderId, String name) {
        ApiClient.Response response = owner.post("/api/works",
                body("name", name, "type", "Site", "folderId", folderId, "url", "example.com/patterns"));
        assertTrue(response.isOk(), "creating site work failed: " + response.raw());
        return response.id();
    }

    protected String createPatternWork(ApiClient owner, String folderId, String name) {
        ApiClient.Response response = owner.post("/api/works",
                body("name", name, "type", "Pattern", "folderId", folderId, "width", 5, "height", 5));
        assertTrue(response.isOk(), "creating matrix work failed: " + response.raw());
        return response.id();
    }

    protected String createColor(ApiClient owner, String name, String hex) {
        ApiClient.Response response = owner.post("/api/yarn-colors", body("name", name, "hexValue", hex, "notes", null));
        assertTrue(response.isOk(), "creating color failed: " + response.raw());
        String id = response.id();
        cleanup.push(() -> owner.delete("/api/yarn-colors/" + id));
        return id;
    }

    protected void share(ApiClient owner, ApiClient member, String workId, String permission) {
        ApiClient.Response invitation = owner.put("/api/works/" + workId + "/sharing/invitation", body("permission", permission));
        assertTrue(invitation.isOk(), "creating invitation failed: " + invitation.raw());
        String code = invitation.json().get("code").asText();

        ApiClient.Response joined = member.post("/api/works/join", body("code", code));
        assertTrue(joined.isOk(), "joining failed: " + joined.raw());
        // The member leaves first, so that deleting the owner's folder really deletes the work instead of handing it over.
        cleanup.push(() -> member.delete("/api/works/" + workId + "/sharing/membership"));
    }

    protected void onCleanup(Runnable action) {
        cleanup.push(action);
    }
}
