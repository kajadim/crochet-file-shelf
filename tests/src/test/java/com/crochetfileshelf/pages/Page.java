package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;
import java.util.List;

/** Small helpers shared by all page objects. */
public abstract class Page {

    protected final WebDriver driver;
    protected final WebDriverWait wait;

    protected Page(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(12));
    }

    protected WebElement visible(By locator) {
        return wait.until(ExpectedConditions.visibilityOfElementLocated(locator));
    }

    protected WebElement clickable(By locator) {
        return wait.until(ExpectedConditions.elementToBeClickable(locator));
    }

    protected List<WebElement> all(By locator) {
        return driver.findElements(locator);
    }

    protected boolean isPresent(By locator) {
        return !driver.findElements(locator).isEmpty();
    }

    protected void type(By locator, String text) {
        WebElement element = visible(locator);
        element.clear();
        element.sendKeys(text);
    }

    protected void clickByText(By locator, String text) {
        wait.until(d -> d.findElements(locator).stream()
                .filter(WebElement::isDisplayed)
                .filter(element -> element.getText().trim().contains(text))
                .findFirst()
                .map(element -> {
                    element.click();
                    return true;
                })
                .orElse(false));
    }

    protected void jsClick(WebElement element) {
        ((JavascriptExecutor) driver).executeScript("arguments[0].click();", element);
    }

    protected void waitUntilGone(By locator) {
        wait.until(ExpectedConditions.invisibilityOfElementLocated(locator));
    }

    protected void waitForText(By locator, String text) {
        wait.until(ExpectedConditions.textToBePresentInElementLocated(locator, text));
    }

    /** Submits the dialog that is open (create, save, ...). */
    protected void submitDialog() {
        clickable(By.cssSelector(".p-dialog button[type='submit']")).click();
    }

    protected void waitForDialogToClose() {
        waitUntilGone(By.cssSelector(".p-dialog"));
    }

    /** Clicks the button with the given label inside the open confirmation dialog. */
    protected void confirmInDialog(String label) {
        clickByText(By.cssSelector(".p-dialog button"), label);
        waitForDialogToClose();
    }
}
