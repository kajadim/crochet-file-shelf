package com.crochetfileshelf.api;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: who can see and change a work")
public class WorkPermissionsApiTest extends ApiTestBase {

    @Test
    @DisplayName("A private work cannot be read, changed, commented on or deleted by another user")
    void privateWorkIsInvisibleToOthers() {
        String folderId = createFolder(alice, unique("Private"), null);
        String workId = createSiteWork(alice, folderId, "Secret site");
        String path = "/api/works/" + workId;

        assertEquals(404, bob.get(path).status());
        assertEquals(404, bob.put(path, body("name", "Stolen", "description", null)).status());
        assertEquals(404, bob.delete(path).status());
        assertEquals(404, bob.get(path + "/comments").status());
        assertEquals(404, bob.post(path + "/comments", body("text", "<p>hello</p>")).status());
        assertEquals(404, bob.get(path + "/site").status());
        assertEquals(404, bob.get(path + "/sharing").status());

        assertEquals(200, alice.get(path).status(), "the owner still sees it");
    }

    @Test
    @DisplayName("Other users' works never show up in a user's own lists")
    void listsOnlyContainOwnWorks() {
        String folderId = createFolder(alice, unique("Lists"), null);
        String name = unique("Only mine");
        createSiteWork(alice, folderId, name);

        assertTrue(alice.get("/api/works").raw().contains(name));
        assertFalse(bob.get("/api/works").raw().contains(name));
        assertFalse(bob.get("/api/works/shared").raw().contains(name));
        assertFalse(bob.get("/api/works?search=" + name.replace(" ", "%20")).raw().contains(name));
    }

    @Test
    @DisplayName("A view-only member can read but not change anything")
    void viewOnlyMemberCanOnlyRead() {
        String folderId = createFolder(alice, unique("ViewOnly"), null);
        String workId = createPatternWork(alice, folderId, "Shared matrix");
        share(alice, bob, workId, "ViewOnly");
        String path = "/api/works/" + workId;

        assertEquals(200, bob.get(path).status());
        assertEquals(200, bob.get(path + "/pattern").status());
        assertEquals(200, bob.get(path + "/comments").status());
        assertTrue(bob.get("/api/works/shared").raw().contains("Shared matrix"));

        assertEquals(403, bob.put(path, body("name", "Renamed", "description", null)).status());
        assertEquals(403, bob.post(path + "/comments", body("text", "<p>hi</p>")).status());
        assertEquals(403, bob.put(path + "/pattern/cells", body("cells", java.util.List.of(
                body("row", 0, "column", 0, "colorId", null)))).status());
        assertEquals(403, bob.put(path + "/pattern/expand", body("top", 1, "bottom", 0, "left", 0, "right", 0)).status());
        assertEquals(403, bob.delete(path).status());
    }

    @Test
    @DisplayName("An editor can comment and resize the matrix, but not rename, share or delete it")
    void editorHasLimitedPowers() {
        String folderId = createFolder(alice, unique("Editor"), null);
        String workId = createPatternWork(alice, folderId, "Editable matrix");
        share(alice, bob, workId, "CanEdit");
        String path = "/api/works/" + workId;

        assertTrue(bob.post(path + "/comments", body("text", "<p>from the editor</p>")).isOk());
        assertTrue(bob.put(path + "/pattern/expand", body("top", 1, "bottom", 0, "left", 0, "right", 0)).isOk());
        assertEquals(6, bob.get(path + "/pattern").json().get("height").asInt());

        assertEquals(403, bob.put(path, body("name", "Renamed", "description", null)).status());
        assertEquals(403, bob.get(path + "/sharing").status());
        assertEquals(403, bob.put(path + "/sharing/invitation", body("permission", "CanEdit")).status());
        assertEquals(403, bob.delete(path).status());
    }

    @Test
    @DisplayName("Only the owner can delete a shared work")
    void ownerCanDeleteSharedWork() {
        String folderId = createFolder(alice, unique("Delete"), null);
        String workId = createSiteWork(alice, folderId, "To be deleted");
        share(alice, bob, workId, "CanEdit");

        assertEquals(403, bob.delete("/api/works/" + workId).status());
        assertTrue(alice.delete("/api/works/" + workId).isOk());
        assertEquals(404, alice.get("/api/works/" + workId).status());
    }

    @Test
    @DisplayName("Removing a member takes their access away immediately")
    void removedMemberLosesAccess() {
        String folderId = createFolder(alice, unique("Revoke"), null);
        String workId = createSiteWork(alice, folderId, "Revocable");
        share(alice, bob, workId, "CanEdit");
        assertEquals(200, bob.get("/api/works/" + workId).status());

        ApiClient.Response sharing = alice.get("/api/works/" + workId + "/sharing");
        String bobId = sharing.json().get("members").get(0).get("userId").asText();
        assertTrue(alice.delete("/api/works/" + workId + "/sharing/members/" + bobId).isOk());

        assertEquals(404, bob.get("/api/works/" + workId).status());
    }

    @Test
    @DisplayName("A member can leave a work and then loses access")
    void memberCanLeave() {
        String folderId = createFolder(alice, unique("Leave"), null);
        String workId = createSiteWork(alice, folderId, "Leavable");
        share(alice, bob, workId, "ViewOnly");

        assertTrue(bob.delete("/api/works/" + workId + "/sharing/membership").isOk());
        assertEquals(404, bob.get("/api/works/" + workId).status());
        assertEquals(200, alice.get("/api/works/" + workId).status());
    }

    @Test
    @DisplayName("Permission changes apply right away")
    void permissionChangeTakesEffect() {
        String folderId = createFolder(alice, unique("Upgrade"), null);
        String workId = createSiteWork(alice, folderId, "Upgradable");
        share(alice, bob, workId, "ViewOnly");
        String path = "/api/works/" + workId;

        assertEquals(403, bob.post(path + "/comments", body("text", "<p>nope</p>")).status());

        String bobId = alice.get(path + "/sharing").json().get("members").get(0).get("userId").asText();
        assertTrue(alice.put(path + "/sharing/members/" + bobId, body("permission", "CanEdit")).isOk());

        assertTrue(bob.post(path + "/comments", body("text", "<p>now allowed</p>")).isOk());
    }

    @Test
    @DisplayName("Invitation codes: invalid, own work and repeated joins are refused")
    void invitationRules() {
        String folderId = createFolder(alice, unique("Invite"), null);
        String workId = createSiteWork(alice, folderId, "Invitable");

        assertEquals(404, bob.post("/api/works/join", body("code", "NOT-A-CODE")).status());

        String code = alice.put("/api/works/" + workId + "/sharing/invitation", body("permission", "ViewOnly"))
                .json().get("code").asText();
        assertEquals(409, alice.post("/api/works/join", body("code", code)).status(), "cannot join your own work");

        assertTrue(bob.post("/api/works/join", body("code", code)).isOk());
        assertEquals(409, bob.post("/api/works/join", body("code", code)).status(), "already a member");

        assertTrue(alice.delete("/api/works/" + workId + "/sharing/invitation").isOk());
        assertEquals(404, ApiClient.loginAs(
                com.crochetfileshelf.base.TestConfig.USER2_EMAIL, com.crochetfileshelf.base.TestConfig.USER2_PASSWORD)
                .post("/api/works/join", body("code", code)).status(), "a revoked code no longer works");
    }
}
