package com.crochetfileshelf.base;

import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.extension.ExtendWith;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.edge.EdgeDriver;
import org.openqa.selenium.edge.EdgeOptions;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxOptions;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

/**
 * Base class for browser tests. Every test gets a fresh browser, so no login or setting leaks between tests.
 * Waits are explicit (no implicit wait) so timeouts do not add up in ways that are hard to predict.
 */
@Tag("ui")
@ExtendWith(ScreenshotOnFailure.class)
public abstract class BaseUiTest extends TestDataBase {

    protected static final Duration TIMEOUT = Duration.ofSeconds(12);

    protected WebDriver driver;
    protected WebDriverWait wait;

    @BeforeEach
    void openBrowser() {
        driver = createDriver();
        driver.manage().window().setSize(new org.openqa.selenium.Dimension(1440, 900));
        wait = new WebDriverWait(driver, TIMEOUT);
    }

    @AfterEach
    void closeBrowser() {
        if (driver != null) {
            driver.quit();
        }
    }

    private static WebDriver createDriver() {
        if (!TestConfig.REMOTE_URL.isEmpty()) {
            return createRemoteDriver();
        }

        switch (TestConfig.BROWSER) {
            case "firefox": {
                FirefoxOptions options = new FirefoxOptions();
                if (TestConfig.HEADLESS) {
                    options.addArguments("-headless");
                }
                return new FirefoxDriver(options);
            }
            case "edge": {
                EdgeOptions options = new EdgeOptions();
                if (TestConfig.HEADLESS) {
                    options.addArguments("--headless=new");
                }
                options.addArguments("--window-size=1440,900");
                return new EdgeDriver(options);
            }
            default: {
                ChromeOptions options = new ChromeOptions();
                if (TestConfig.HEADLESS) {
                    options.addArguments("--headless=new");
                }
                options.addArguments("--window-size=1440,900");
                quietChrome(options);
                return new ChromeDriver(options);
            }
        }
    }

    /** Keeps Chrome's own pop-ups (save password, password breach warning) from taking the keyboard and mouse. */
    private static void quietChrome(ChromeOptions options) {
        options.setExperimentalOption("prefs", java.util.Map.of(
                "credentials_enable_service", false,
                "profile.password_manager_enabled", false,
                "profile.password_manager_leak_detection", false));
        options.addArguments("--disable-features=PasswordLeakDetection,PasswordCheck");
    }

    /** Used when the browser runs in its own container (Selenium Grid), for example when the tests run in Docker. */
    private static WebDriver createRemoteDriver() {
        try {
            java.net.URL url = java.net.URI.create(TestConfig.REMOTE_URL).toURL();
            switch (TestConfig.BROWSER) {
                case "firefox":
                    return new org.openqa.selenium.remote.RemoteWebDriver(url, new FirefoxOptions());
                case "edge":
                    return new org.openqa.selenium.remote.RemoteWebDriver(url, new EdgeOptions());
                default: {
                    ChromeOptions options = new ChromeOptions();
                    options.addArguments("--window-size=1440,900");
                    quietChrome(options);
                    return new org.openqa.selenium.remote.RemoteWebDriver(url, options);
                }
            }
        } catch (java.net.MalformedURLException e) {
            throw new IllegalStateException("Invalid remote.url: " + TestConfig.REMOTE_URL, e);
        }
    }

    protected WebDriver driver() {
        return driver;
    }

    protected void open(String path) {
        driver.get(TestConfig.BASE_URL + path);
    }

    protected WebElement visible(By locator) {
        return wait.until(ExpectedConditions.visibilityOfElementLocated(locator));
    }

    protected WebElement clickable(By locator) {
        return wait.until(ExpectedConditions.elementToBeClickable(locator));
    }

    protected boolean isPresent(By locator) {
        return !driver.findElements(locator).isEmpty();
    }

    protected void waitForUrlContains(String fragment) {
        wait.until(ExpectedConditions.urlContains(fragment));
    }

    protected void waitForText(By locator, String text) {
        wait.until(ExpectedConditions.textToBePresentInElementLocated(locator, text));
    }

    protected void waitUntilGone(By locator) {
        wait.until(ExpectedConditions.invisibilityOfElementLocated(locator));
    }

    protected void jsClick(WebElement element) {
        ((JavascriptExecutor) driver).executeScript("arguments[0].click();", element);
    }

    /** Clicks the first element that matches the locator and contains the text. */
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

    protected void logInThroughTheForm(String email, String password) {
        open("/login");
        visible(By.id("email")).sendKeys(email);
        visible(By.id("password")).sendKeys(password);
        clickable(By.cssSelector(".auth-card__form button[type='submit']")).click();
        visible(By.cssSelector(".topbar"));
    }

    protected void logInAsFirstUser() {
        logInThroughTheForm(TestConfig.USER1_EMAIL, TestConfig.USER1_PASSWORD);
    }

    protected void logInAsSecondUser() {
        logInThroughTheForm(TestConfig.USER2_EMAIL, TestConfig.USER2_PASSWORD);
    }
}
