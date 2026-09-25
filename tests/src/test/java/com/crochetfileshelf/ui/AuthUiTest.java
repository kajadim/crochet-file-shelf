package com.crochetfileshelf.ui;

import com.crochetfileshelf.base.BaseUiTest;
import com.crochetfileshelf.base.TestConfig;
import com.crochetfileshelf.pages.DashboardPage;
import com.crochetfileshelf.pages.LoginPage;
import com.crochetfileshelf.pages.RegisterPage;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.By;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("UI: signing in and registration")
public class AuthUiTest extends BaseUiTest {

    @Test
    @DisplayName("Logging in with correct data opens the dashboard")
    void successfulLogin() {
        open("/login");
        new LoginPage(driver).loginWith(TestConfig.USER1_EMAIL, TestConfig.USER1_PASSWORD);

        DashboardPage dashboard = new DashboardPage(driver);
        assertFalse(dashboard.signedInName().isBlank());
        assertFalse(driver.getCurrentUrl().contains("/login"));
    }

    @Test
    @DisplayName("A wrong password shows an error and keeps the user on the login page")
    void wrongPassword() {
        open("/login");
        LoginPage login = new LoginPage(driver).loginWith(TestConfig.USER1_EMAIL, "definitely-wrong");

        assertTrue(login.errorIsShown());
        assertTrue(driver.getCurrentUrl().contains("/login"));
    }

    @Test
    @DisplayName("An unknown email shows the same kind of error")
    void unknownEmail() {
        open("/login");
        LoginPage login = new LoginPage(driver).loginWith("nobody-" + System.nanoTime() + "@example.com", "Whatever123");

        assertTrue(login.errorIsShown());
    }

    @Test
    @DisplayName("Pages behind the login redirect a guest to the login page")
    void authGuardRedirects() {
        open("/palette");
        waitForUrlContains("/login");

        open("/profile");
        waitForUrlContains("/login");
    }

    @Test
    @DisplayName("A signed-in user is sent away from the login page")
    void guestGuardRedirects() {
        logInAsFirstUser();

        open("/login");
        wait.until(d -> !d.getCurrentUrl().contains("/login"));
    }

    @Test
    @DisplayName("The language chosen on the login page is applied and remembered")
    void languageSelection() {
        open("/login");
        LoginPage login = new LoginPage(driver);
        String english = login.title();

        login.chooseLanguage("Srpski");
        wait.until(d -> !login.title().equals(english));
        assertEquals("sr", driver.findElement(By.tagName("html")).getAttribute("lang"));

        driver.navigate().refresh();
        LoginPage afterReload = new LoginPage(driver);
        assertNotEquals(english, afterReload.title(), "the choice survives a reload");
    }

    @Test
    @DisplayName("Registration validates the form before anything is sent")
    void registrationValidation() {
        open("/register");
        RegisterPage register = new RegisterPage(driver);

        register.touchAllFields();
        assertFalse(register.fieldErrors().isEmpty(), "empty required fields show errors");

        register.fill("Ana", "Anic", "a", "not-an-email", "short");
        register.touchAllFields();
        assertTrue(register.fieldErrors().size() >= 2, "a bad username, email and password are all flagged");
    }

    @Test
    @DisplayName("Registration tells right away that a username is already taken")
    void takenUsernameIsReportedWhileTyping() {
        open("/register");
        RegisterPage register = new RegisterPage(driver);

        register.fill("Ana", "Anic", TestConfig.USER1_USERNAME, "someone-new@example.com", "LongEnough123");
        register.waitForFieldError("already taken");
    }

    @Test
    @DisplayName("Registering with an existing email is refused with a message")
    void existingEmailIsRefused() {
        open("/register");
        RegisterPage register = new RegisterPage(driver);

        register.fill("Ana", "Anic", "newname" + System.nanoTime() % 100000, TestConfig.USER1_EMAIL, "LongEnough123");
        register.submit();

        assertFalse(register.serverError().isBlank());
        assertTrue(driver.getCurrentUrl().contains("/register"));
    }
}
