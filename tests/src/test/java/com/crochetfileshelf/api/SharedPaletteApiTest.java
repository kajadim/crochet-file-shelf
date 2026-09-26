package com.crochetfileshelf.api;

import com.fasterxml.jackson.databind.JsonNode;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.List;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: colors on a shared matrix")
public class SharedPaletteApiTest extends ApiTestBase {

    private String paint(ApiClient client, String workId, int row, int column, String colorId) {
        ApiClient.Response response = client.put("/api/works/" + workId + "/pattern/cells",
                body("cells", List.of(body("row", row, "column", column, "colorId", colorId))));
        return response.status() + "";
    }

    private JsonNode colorNamed(ApiClient client, String workId, String name) {
        for (JsonNode color : client.get("/api/works/" + workId + "/pattern/colors").json()) {
            if (color.get("name").asText().equals(name)) {
                return color;
            }
        }
        return null;
    }

    @Test
    @DisplayName("An editor sees the owner's colors, can paint with them and copy them to their own palette")
    void editorUsesAndCopiesSharedColors() {
        String colorName = unique("Shared rose");
        String colorId = createColor(alice, colorName, "#C08497");
        String workId = createPatternWork(alice, createFolder(alice, unique("Palette"), null), "Shared colors");
        assertEquals("200", paint(alice, workId, 0, 0, colorId));
        share(alice, bob, workId, "CanEdit");

        JsonNode asBob = colorNamed(bob, workId, colorName);
        assertNotNull(asBob, "the color used in the work is listed for the member");
        assertFalse(asBob.get("isMine").asBoolean());
        assertTrue(colorNamed(alice, workId, colorName).get("isMine").asBoolean());

        assertEquals("200", paint(bob, workId, 1, 1, colorId), "a member paints with a color that is already used in the work");

        ApiClient.Response copy = bob.post("/api/works/" + workId + "/pattern/colors/" + colorId + "/copy", body());
        assertTrue(copy.isOk(), copy.raw());
        String copyId = copy.id();
        onCleanup(() -> bob.delete("/api/yarn-colors/" + copyId));
        assertNotEquals(colorId, copyId);
        assertTrue(bob.get("/api/yarn-colors").raw().contains(colorName));

        ApiClient.Response again = bob.post("/api/works/" + workId + "/pattern/colors/" + colorId + "/copy", body());
        assertEquals(copyId, again.id(), "copying twice reuses the color that is already in the palette");
    }

    @Test
    @DisplayName("Colors that are not used in the work stay private, and a viewer can look but not paint")
    void privateColorsAndViewers() {
        String usedId = createColor(alice, unique("Used"), "#112233");
        String privateId = createColor(alice, unique("Private"), "#445566");
        String workId = createPatternWork(alice, createFolder(alice, unique("Private palette"), null), "Colors");
        assertEquals("200", paint(alice, workId, 0, 0, usedId));
        share(alice, bob, workId, "CanEdit");

        assertEquals("404", paint(bob, workId, 2, 2, privateId), "a color that is not part of the work cannot be used");
        assertEquals(404, bob.post("/api/works/" + workId + "/pattern/colors/" + privateId + "/copy", body()).status());

        String viewerWork = createPatternWork(alice, createFolder(alice, unique("Viewer palette"), null), "Viewer colors");
        assertEquals("200", paint(alice, viewerWork, 0, 0, usedId));
        share(alice, bob, viewerWork, "ViewOnly");
        assertEquals(200, bob.get("/api/works/" + viewerWork + "/pattern/colors").status());
        assertEquals("403", paint(bob, viewerWork, 1, 1, usedId));
    }

    @Test
    @DisplayName("When the owner deletes a shared work, the new owner gets the colors it uses")
    void colorsFollowTheWorkToItsNewOwner() {
        String colorName = unique("Inherited");
        String colorId = createColor(alice, colorName, "#ABCDEF");
        String workId = createPatternWork(alice, createFolder(alice, unique("Handover"), null), "Handover");
        assertEquals("200", paint(alice, workId, 0, 0, colorId));
        share(alice, bob, workId, "CanEdit");
        onCleanup(() -> {
            JsonNode mine = bob.get("/api/yarn-colors?search=" + colorName.replace(" ", "%20")).json();
            mine.forEach(color -> bob.delete("/api/yarn-colors/" + color.get("id").asText()));
        });
        onCleanup(() -> bob.delete("/api/works/" + workId));

        assertTrue(alice.delete("/api/works/" + workId).isOk());

        assertEquals(200, bob.get("/api/works/" + workId).status(), "the editor now owns the work");
        JsonNode color = colorNamed(bob, workId, colorName);
        assertNotNull(color);
        assertTrue(color.get("isMine").asBoolean(), "the color now belongs to the new owner");
        assertTrue(bob.get("/api/yarn-colors").raw().contains(colorName));
        assertEquals("200", paint(bob, workId, 3, 3, color.get("id").asText()));
    }
}
