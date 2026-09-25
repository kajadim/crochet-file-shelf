package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

import java.util.List;
import java.util.stream.Collectors;

public class RegisterPage extends Page {

    public RegisterPage(WebDriver driver) {
        super(driver);
        visible(By.id("firstName"));
    }

    public RegisterPage fill(String firstName, String lastName, String username, String email, String password) {
        type(By.id("firstName"), firstName);
        type(By.id("lastName"), lastName);
        type(By.id("username"), username);
        type(By.id("email"), email);
        type(By.id("password"), password);
        return this;
    }

    public void submit() {
        clickable(By.cssSelector(".auth-card__form button[type='submit']")).click();
    }

    public void touchAllFields() {
        for (String id : List.of("firstName", "lastName", "username", "email", "password")) {
            visible(By.id(id)).click();
        }
        visible(By.cssSelector(".auth-card__title")).click();
    }

    public List<String> fieldErrors() {
        return all(By.cssSelector(".field__error")).stream()
                .map(element -> element.getText().trim())
                .filter(text -> !text.isEmpty())
                .collect(Collectors.toList());
    }

    public void waitForFieldError(String text) {
        waitForText(By.cssSelector(".auth-card"), text);
    }

    public String serverError() {
        return visible(By.cssSelector(".auth-card .alert--error")).getText();
    }
}
