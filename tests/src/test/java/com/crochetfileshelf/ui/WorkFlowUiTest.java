package com.crochetfileshelf.ui;

import com.crochetfileshelf.base.BaseUiTest;
import com.crochetfileshelf.pages.DashboardPage;
import com.crochetfileshelf.pages.SiteWorkPage;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("UI: a work from creation to deletion")
public class WorkFlowUiTest extends BaseUiTest {

    @Test
    @DisplayName("Create a folder and a site work, comment on it, then delete both")
    void folderWorkCommentAndCleanUp() {
        String folderName = unique("E2E folder");
        String workName = unique("E2E site");
        String comment = "First comment " + System.nanoTime() % 100000;

        logInAsFirstUser();
        DashboardPage dashboard = new DashboardPage(driver);

        dashboard.createFolder(folderName);
        assertTrue(dashboard.folderIsInTree(folderName));
        assertEquals(folderName, dashboard.heading());

        dashboard.createSiteWork(workName, "example.com/crochet");
        assertTrue(dashboard.workNames().contains(workName));

        dashboard.openWork(workName);
        SiteWorkPage site = new SiteWorkPage(driver);
        assertEquals(workName, site.name());
        assertEquals("example.com", site.domain());

        site.addComment(comment);
        assertTrue(site.commentTexts().stream().anyMatch(text -> text.contains(comment)));

        site.goBack();
        dashboard = new DashboardPage(driver);
        dashboard.openFolderFromTree(folderName);
        dashboard.deleteWork(workName);
        assertFalse(dashboard.workNames().contains(workName));

        dashboard.deleteFolderFromTree(folderName);
        assertFalse(dashboard.folderIsInTree(folderName));
    }

    @Test
    @DisplayName("A subfolder card opens the subfolder, and works can be moved into it")
    void subfolderNavigation() {
        String parent = unique("E2E parent");
        String child = unique("E2E child");
        String parentId = createFolder(alice, parent, null);
        createFolder(alice, child, parentId);
        String workName = unique("E2E movable");
        createSiteWork(alice, parentId, workName);

        logInAsFirstUser();
        DashboardPage dashboard = new DashboardPage(driver);
        dashboard.openFolderFromTree(parent);

        assertTrue(dashboard.workNames().contains(workName));
        org.junit.jupiter.api.Assertions.assertTrue(
                driver.findElements(org.openqa.selenium.By.cssSelector(".folder-card__name")).stream()
                        .anyMatch(element -> element.getText().equals(child)),
                "the subfolder is listed as a card above the works");

        org.openqa.selenium.WebElement card = driver.findElements(org.openqa.selenium.By.cssSelector(".folder-card")).get(0);
        card.click();
        wait.until(d -> dashboard.heading().contains(child));
    }
}
