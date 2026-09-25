package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

public class LoginPage extends Page {

    private final By email = By.id("email");
    private final By password = By.id("password");
    private final By submit = By.cssSelector(".auth-card__form button[type='submit']");
    private final By error = By.cssSelector(".auth-card .alert--error");
    private final By languageSelect = By.cssSelector("app-language-select select");

    public LoginPage(WebDriver driver) {
        super(driver);
        visible(email);
    }

    public LoginPage loginWith(String emailValue, String passwordValue) {
        type(email, emailValue);
        type(password, passwordValue);
        clickable(submit).click();
        return this;
    }

    public boolean errorIsShown() {
        return visible(error).isDisplayed();
    }

    public String errorText() {
        return visible(error).getText();
    }

    public String title() {
        return visible(By.cssSelector(".auth-card__title")).getText();
    }

    public void chooseLanguage(String label) {
        new org.openqa.selenium.support.ui.Select(visible(languageSelect)).selectByVisibleText(label);
    }

    public void goToRegister() {
        clickByText(By.cssSelector(".auth-card__links a"), "Create an account");
    }
}
