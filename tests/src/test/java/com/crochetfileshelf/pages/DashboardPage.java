package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

import java.util.List;
import java.util.stream.Collectors;

public class DashboardPage extends Page {

    private final By title = By.cssSelector(".dashboard__title");
    private final By workTitles = By.cssSelector(".work-card__title");

    public DashboardPage(WebDriver driver) {
        super(driver);
        visible(title);
    }

    public String heading() {
        return visible(title).getText();
    }

    public String signedInName() {
        return visible(By.cssSelector(".topbar__profile-name")).getText();
    }

    // ---- folders

    public DashboardPage createFolder(String name) {
        clickByText(By.cssSelector(".dashboard__sidebar button"), "New folder");
        type(By.id("folder-name"), name);
        submitDialog();
        waitForDialogToClose();
        waitForText(title, name);
        return this;
    }

    public boolean folderIsInTree(String name) {
        return all(By.cssSelector(".tree__name")).stream().anyMatch(element -> element.getText().equals(name));
    }

    public void openFolderFromTree(String name) {
        clickByText(By.cssSelector(".tree__label"), name);
        waitForText(title, name);
    }

    public void deleteFolderFromTree(String name) {
        List<org.openqa.selenium.WebElement> rows = all(By.cssSelector(".tree__row"));
        for (org.openqa.selenium.WebElement row : rows) {
            if (row.getText().contains(name)) {
                row.findElement(By.cssSelector(".action-menu__trigger")).click();
                break;
            }
        }
        clickByText(By.cssSelector(".action-menu__item"), "Delete");
        confirmInDialog("Delete");
        wait.until(d -> !folderIsInTree(name));
    }

    public void showAllWorks() {
        clickByText(By.cssSelector(".dashboard__nav-item"), "All works");
    }

    public void showSharedWithMe() {
        clickByText(By.cssSelector(".dashboard__nav-item"), "Shared with me");
        waitForText(title, "Shared with me");
    }

    // ---- works

    public DashboardPage createSiteWork(String name, String url) {
        clickable(By.cssSelector(".dashboard__new-work button")).click();
        jsClick(wait.until(org.openqa.selenium.support.ui.ExpectedConditions.presenceOfElementLocated(By.id("type-site"))));
        type(By.id("work-url"), url);
        type(By.id("work-name"), name);
        submitDialog();
        waitForDialogToClose();
        waitForWork(name);
        return this;
    }

    public void waitForWork(String name) {
        wait.until(d -> workNames().contains(name));
    }

    public List<String> workNames() {
        return all(workTitles).stream().map(element -> element.getText().trim()).collect(Collectors.toList());
    }

    public void openWork(String name) {
        clickByText(workTitles, name);
    }

    public void deleteWork(String name) {
        for (org.openqa.selenium.WebElement card : all(By.cssSelector(".work-card"))) {
            if (card.getText().contains(name)) {
                card.findElement(By.cssSelector(".action-menu__trigger")).click();
                break;
            }
        }
        clickByText(By.cssSelector(".action-menu__item"), "Delete");
        confirmInDialog("Delete");
        wait.until(d -> !workNames().contains(name));
    }

    public boolean cardShowsSharedBadge(String name) {
        return all(By.cssSelector(".work-card")).stream()
                .anyMatch(card -> card.getText().contains(name) && !card.findElements(By.cssSelector(".work-card__shared")).isEmpty());
    }

    // ---- joining a shared work

    public void joinWithCode(String code) {
        clickByText(By.cssSelector(".dashboard__sidebar button"), "Join a work");
        type(By.id("join-code"), code);
        submitDialog();
        waitForDialogToClose();
    }

    // ---- notifications

    public void openNotifications() {
        clickable(By.cssSelector(".bell__button")).click();
        visible(By.cssSelector(".bell__panel"));
    }

    public List<String> notificationTexts() {
        return all(By.cssSelector(".bell__item-text")).stream().map(element -> element.getText().trim()).collect(Collectors.toList());
    }

    public void openNotificationLink(String workName) {
        clickByText(By.cssSelector(".bell__link"), workName);
    }
}
