package com.crochetfileshelf.flows;

import com.crochetfileshelf.base.BaseUiTest;
import com.crochetfileshelf.pages.DashboardPage;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.By;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

/**
 * A flow that crosses both sides: the data is prepared and changed through the REST API (the backend),
 * while the second user checks what that means in the browser (the frontend).
 */
@Tag("flow")
@DisplayName("Flow: sharing a work between two users")
public class SharingFlowTest extends BaseUiTest {

    @Test
    @DisplayName("An invitation code is joined in the browser, the work shows up, and a comment notifies the new member")
    void invitationJoinAndNotification() {
        String workName = unique("Flow matrix");
        String folderId = createFolder(alice, unique("Flow folder"), null);
        String workId = createPatternWork(alice, folderId, workName);
        onCleanup(() -> bob.delete("/api/works/" + workId + "/sharing/membership"));
        String code = alice.put("/api/works/" + workId + "/sharing/invitation", body("permission", "CanEdit"))
                .json().get("code").asText();

        logInAsSecondUser();
        DashboardPage dashboard = new DashboardPage(driver);

        dashboard.joinWithCode(code);
        dashboard.showSharedWithMe();
        dashboard.waitForWork(workName);
        assertTrue(dashboard.workNames().contains(workName));

        dashboard.openWork(workName);
        waitForUrlContains("/works/" + workId);
        visible(By.cssSelector(".matrix__name"));
        assertTrue(driver.findElements(By.cssSelector(".matrix__toggle-label")).stream()
                        .anyMatch(element -> element.getText().equals("Add cells")),
                "an editor sees the tools for changing the matrix");

        assertTrue(alice.post("/api/works/" + workId + "/comments", body("text", "<p>Welcome aboard</p>")).isOk());
        assertTrue(bob.get("/api/notifications").raw().contains(workName));

        open("/");
        DashboardPage home = new DashboardPage(driver);
        home.openNotifications();
        wait.until(d -> home.notificationTexts().stream().anyMatch(text -> text.contains("commented on")));

        home.openNotificationLink(workName);
        waitForUrlContains("/works/" + workId);
    }

    @Test
    @DisplayName("The owner sees a Shared badge once somebody has joined")
    void ownerSeesSharedBadge() {
        String workName = unique("Flow badge");
        String folderId = createFolder(alice, unique("Flow badge folder"), null);
        String workId = createSiteWork(alice, folderId, workName);

        logInAsFirstUser();
        DashboardPage dashboard = new DashboardPage(driver);
        dashboard.waitForWork(workName);
        assertFalse(dashboard.cardShowsSharedBadge(workName), "not shared yet");

        share(alice, bob, workId, "ViewOnly");
        driver.navigate().refresh();
        dashboard = new DashboardPage(driver);
        dashboard.waitForWork(workName);
        assertTrue(dashboard.cardShowsSharedBadge(workName));
    }
}
