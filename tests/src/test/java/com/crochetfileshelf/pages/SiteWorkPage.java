package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

import java.util.List;
import java.util.stream.Collectors;

public class SiteWorkPage extends Page {

    public SiteWorkPage(WebDriver driver) {
        super(driver);
        visible(By.cssSelector(".site__name"));
    }

    public String name() {
        return visible(By.cssSelector(".site__name")).getText();
    }

    public String domain() {
        return visible(By.cssSelector(".site__domain")).getText();
    }

    public SiteWorkPage addComment(String text) {
        clickByText(By.cssSelector(".comments__toggle"), "Add comment");
        visible(By.cssSelector(".ql-editor")).sendKeys(text);
        clickable(By.cssSelector(".comments__submit button")).click();
        wait.until(d -> commentTexts().stream().anyMatch(comment -> comment.contains(text)));
        return this;
    }

    public List<String> commentTexts() {
        return all(By.cssSelector(".comment__body")).stream().map(element -> element.getText().trim()).collect(Collectors.toList());
    }

    public void goBack() {
        clickable(By.cssSelector(".site__back")).click();
    }
}
