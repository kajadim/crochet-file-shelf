package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.Select;

public class ProfilePage extends Page {

    public ProfilePage(WebDriver driver) {
        super(driver);
        visible(By.cssSelector(".profile__name"));
    }

    public String username() {
        return visible(By.cssSelector(".profile__meta")).getText();
    }

    public void editBio(String bio) {
        clickByText(By.cssSelector(".profile__identity button"), "Edit profile");
        type(By.id("profile-bio"), bio);
        clickable(By.cssSelector(".profile__form-actions button[type='submit']")).click();
        waitForText(By.cssSelector(".profile__card"), bio);
    }

    public String bio() {
        return visible(By.cssSelector(".profile__bio")).getText();
    }

    public void chooseTheme(String label) {
        new Select(visible(By.id("profile-theme"))).selectByVisibleText(label);
    }

    public String activeTheme() {
        return driver.findElement(By.tagName("html")).getAttribute("data-theme");
    }
}
