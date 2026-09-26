package com.crochetfileshelf.api;

import com.crochetfileshelf.base.TestConfig;
import com.fasterxml.jackson.databind.JsonNode;
import org.junit.jupiter.api.Assumptions;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.List;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: deleting an account")
public class AccountDeletionApiTest extends ApiTestBase {

    private static void paint(ApiClient client, String workId, int row, int column, String colorId) {
        ApiClient.Response response = client.put("/api/works/" + workId + "/pattern/cells",
                body("cells", List.of(body("row", row, "column", column, "colorId", colorId))));
        assertTrue(response.isOk(), response.raw());
    }

    @Test
    @DisplayName("The account, its works and palette go; shared works stay with the people who can edit them")
    void deletingAnAccountHandsSharedWorksOver() {
        Assumptions.assumeFalse(TestConfig.USER3_EMAIL.isEmpty(), "user3 is not configured");
        ApiClient carol = ApiClient.loginAs(TestConfig.USER3_EMAIL, TestConfig.USER3_PASSWORD);

        String carolColorName = unique("Carol red");
        String carolColor = carol.post("/api/yarn-colors", body("name", carolColorName, "hexValue", "#AA0000", "notes", null)).id();

        String ownWork = createSiteWork(carol, createFolder(carol, unique("Carol folder"), null), "Carol only");

        String handedOver = createPatternWork(carol, createFolder(carol, unique("Carol shared"), null), "Handed over");
        paint(carol, handedOver, 0, 0, carolColor);
        share(carol, bob, handedOver, "CanEdit");
        share(carol, alice, handedOver, "ViewOnly");
        onCleanup(() -> bob.delete("/api/works/" + handedOver));

        String aliceWork = createPatternWork(alice, createFolder(alice, unique("Alice matrix"), null), "Alice matrix");
        share(alice, carol, aliceWork, "CanEdit");
        paint(carol, aliceWork, 1, 1, carolColor);
        assertTrue(carol.post("/api/works/" + aliceWork + "/comments", body("text", "<p>Hello from Carol</p>")).isOk());

        assertEquals(400, carol.post("/api/profile/delete", body("password", "definitely-wrong")).status(),
                "the password has to be confirmed");
        assertTrue(carol.post("/api/profile/delete", body("password", TestConfig.USER3_PASSWORD)).isOk());

        assertEquals(401, ApiClient.anonymous().post("/api/auth/login",
                body("email", TestConfig.USER3_EMAIL, "password", TestConfig.USER3_PASSWORD)).status(), "the account is gone");

        assertEquals(404, bob.get("/api/works/" + ownWork).status(), "an unshared work is deleted with the account");
        assertEquals(200, bob.get("/api/works/" + handedOver).status(), "an editor keeps the shared work");
        assertEquals("Owner", bob.get("/api/works/" + handedOver).json().get("role").asText());
        assertEquals(404, alice.get("/api/works/" + handedOver).status(), "a viewer loses it");

        JsonNode inherited = null;
        for (JsonNode color : bob.get("/api/works/" + handedOver + "/pattern/colors").json()) {
            if (color.get("name").asText().equals(carolColorName)) {
                inherited = color;
            }
        }
        assertNotNull(inherited, "the color used by the handed-over work is kept");
        assertTrue(inherited.get("isMine").asBoolean(), "and it now belongs to the new owner");
        String inheritedId = inherited.get("id").asText();
        onCleanup(() -> bob.delete("/api/yarn-colors/" + inheritedId));

        assertEquals(200, alice.get("/api/works/" + aliceWork).status(), "other people's works stay");
        assertEquals(1, alice.get("/api/works/" + aliceWork + "/pattern").json().get("cells").size(),
                "the cell the deleted user painted is still there");
        JsonNode aliceColors = alice.get("/api/works/" + aliceWork + "/pattern/colors").json();
        assertEquals(1, aliceColors.size());
        assertTrue(aliceColors.get(0).get("isMine").asBoolean(), "its color moved to the owner's palette");
        String movedId = aliceColors.get(0).get("id").asText();
        onCleanup(() -> alice.delete("/api/yarn-colors/" + movedId));

        JsonNode comment = alice.get("/api/works/" + aliceWork + "/comments").json().get(0);
        assertTrue(comment.path("authorId").isNull() || comment.path("authorId").isMissingNode(), "comments stay, without an author");
        assertEquals("", comment.get("authorName").asText());
    }
}
