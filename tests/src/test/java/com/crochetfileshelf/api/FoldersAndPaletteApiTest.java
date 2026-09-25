package com.crochetfileshelf.api;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: folders and palette")
public class FoldersAndPaletteApiTest extends ApiTestBase {

    @Test
    @DisplayName("Folders can be nested and renamed")
    void nestedFoldersAndRename() {
        String parent = createFolder(alice, unique("Parent"), null);
        String child = createFolder(alice, "Child", parent);

        ApiClient.Response folders = alice.get("/api/folders");
        assertTrue(folders.raw().contains(child));

        String newName = unique("Renamed");
        ApiClient.Response renamed = alice.put("/api/folders/" + parent, body("name", newName));
        assertTrue(renamed.isOk());
        assertEquals(newName, renamed.json().get("name").asText());
    }

    @Test
    @DisplayName("Two folders with the same name in the same place conflict")
    void duplicateFolderNamesConflict() {
        String name = unique("Same");
        createFolder(alice, name, null);

        ApiClient.Response duplicate = alice.post("/api/folders", body("name", name, "parentFolderId", null));
        assertEquals(409, duplicate.status());
    }

    @Test
    @DisplayName("Deleting a folder deletes the works inside it and reports the numbers first")
    void deletingFolderRemovesContents() {
        String parent = createFolder(alice, unique("Doomed"), null);
        String child = createFolder(alice, "Inside", parent);
        String workId = createSiteWork(alice, child, "Inner work");

        ApiClient.Response summary = alice.get("/api/folders/" + parent + "/deletion-summary");
        assertEquals(1, summary.json().get("subfolderCount").asInt());
        assertEquals(1, summary.json().get("workCount").asInt());

        assertTrue(alice.delete("/api/folders/" + parent).isOk());
        assertEquals(404, alice.get("/api/works/" + workId).status());
    }

    @Test
    @DisplayName("Other users cannot see, rename or delete a folder, or put works into it")
    void foldersBelongToTheirOwner() {
        String folder = createFolder(alice, unique("Alice only"), null);

        assertFalse(bob.get("/api/folders").raw().contains(folder));
        assertEquals(404, bob.put("/api/folders/" + folder, body("name", "Mine now")).status());
        assertEquals(404, bob.delete("/api/folders/" + folder).status());
        assertEquals(404, bob.post("/api/works", body(
                "name", "Sneaky", "type", "Site", "folderId", folder, "url", "example.com")).status());
        assertEquals(404, bob.post("/api/folders", body("name", "Sub", "parentFolderId", folder)).status());
    }

    @Test
    @DisplayName("A work can be moved to another folder of its owner, but not to someone else's")
    void movingWorks() {
        String first = createFolder(alice, unique("From"), null);
        String second = createFolder(alice, unique("To"), null);
        String bobsFolder = createFolder(bob, unique("Bobs"), null);
        String workId = createSiteWork(alice, first, "Traveller");

        assertTrue(alice.put("/api/works/" + workId + "/move", body("folderId", second)).isOk());
        assertEquals(second, alice.get("/api/works/" + workId).json().get("folderId").asText());

        assertEquals(404, alice.put("/api/works/" + workId + "/move", body("folderId", bobsFolder)).status());
    }

    @Test
    @DisplayName("Palette: colors are validated and names must be unique")
    void paletteValidation() {
        String name = unique("Rose");
        createColor(alice, name, "#C08497");

        assertEquals(409, alice.post("/api/yarn-colors", body("name", name, "hexValue", "#112233", "notes", null)).status());
        assertEquals(400, alice.post("/api/yarn-colors", body("name", unique("Bad"), "hexValue", "red", "notes", null)).status());
        assertEquals(400, alice.post("/api/yarn-colors", body("name", "  ", "hexValue", "#112233", "notes", null)).status());
    }

    @Test
    @DisplayName("Palette: search and sorting are done by the server")
    void paletteSearchAndSort() {
        String tag = unique("Sortme");
        createColor(alice, tag + " Cherry", "#D2042D");
        createColor(alice, tag + " Amber", "#FFBF00");
        createColor(alice, tag + " Blue", "#0000FF");

        List<String> byNameAscending = names(alice.get("/api/yarn-colors?search=" + enc(tag) + "&sort=NameAsc"));
        assertEquals(List.of(tag + " Amber", tag + " Blue", tag + " Cherry"), byNameAscending);

        List<String> byNameDescending = names(alice.get("/api/yarn-colors?search=" + enc(tag) + "&sort=NameDesc"));
        assertEquals(List.of(tag + " Cherry", tag + " Blue", tag + " Amber"), byNameDescending);

        List<String> byHexAscending = names(alice.get("/api/yarn-colors?search=" + enc(tag) + "&sort=HexAsc"));
        assertEquals(List.of(tag + " Blue", tag + " Cherry", tag + " Amber"), byHexAscending);

        assertEquals(List.of(tag + " Amber"), names(alice.get("/api/yarn-colors?search=ffbf")), "hex search works without #");
        assertTrue(names(alice.get("/api/yarn-colors?search=" + enc("zzz-no-match-" + tag))).isEmpty());
    }

    @Test
    @DisplayName("Palette: a color belongs to one user")
    void paletteIsPrivate() {
        String name = unique("Private color");
        String colorId = createColor(alice, name, "#123456");

        assertFalse(bob.get("/api/yarn-colors").raw().contains(name));
        assertEquals(404, bob.put("/api/yarn-colors/" + colorId, body("name", "Taken", "hexValue", "#654321", "notes", null)).status());
        assertEquals(404, bob.delete("/api/yarn-colors/" + colorId).status());
        assertEquals(404, bob.get("/api/yarn-colors/" + colorId + "/works").status());
    }

    private static List<String> names(ApiClient.Response response) {
        List<String> result = new ArrayList<>();
        response.json().forEach(node -> result.add(node.get("name").asText()));
        return result;
    }

    private static String enc(String value) {
        return java.net.URLEncoder.encode(value, java.nio.charset.StandardCharsets.UTF_8);
    }
}
