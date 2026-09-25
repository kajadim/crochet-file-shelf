package com.crochetfileshelf.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

import java.util.List;
import java.util.stream.Collectors;

public class PalettePage extends Page {

    private final By colorNames = By.cssSelector(".swatch__name");

    public PalettePage(WebDriver driver) {
        super(driver);
        visible(By.cssSelector(".palette__title"));
    }

    public PalettePage addColor(String name, String hex) {
        clickByText(By.cssSelector(".palette__head button"), "Add color");
        type(By.id("color-name"), name);
        type(By.id("color-hex"), hex);
        submitDialog();
        waitForDialogToClose();
        wait.until(d -> colors().contains(name));
        return this;
    }

    public List<String> colors() {
        return all(colorNames).stream().map(element -> element.getText().trim()).collect(Collectors.toList());
    }

    public void search(String text) {
        type(By.cssSelector(".palette__search-input"), text);
    }

    public void waitForColors(List<String> expected) {
        wait.until(d -> colors().equals(expected));
    }

    public boolean noResultsMessageIsShown() {
        return all(By.cssSelector(".palette__note")).stream().anyMatch(element -> element.getText().contains("No colors match"));
    }

    public void editColorName(String currentName, String newName) {
        openMenuFor(currentName);
        clickByText(By.cssSelector(".action-menu__item"), "Edit");
        type(By.id("color-name"), newName);
        submitDialog();
        waitForDialogToClose();
        wait.until(d -> colors().contains(newName));
    }

    public void deleteColor(String name) {
        openMenuFor(name);
        clickByText(By.cssSelector(".action-menu__item"), "Delete");
        confirmInDialog("Delete");
        wait.until(d -> !colors().contains(name));
    }

    private void openMenuFor(String name) {
        for (org.openqa.selenium.WebElement card : all(By.cssSelector(".swatch"))) {
            if (card.getText().contains(name)) {
                card.findElement(By.cssSelector(".action-menu__trigger")).click();
                return;
            }
        }
        throw new IllegalStateException("No color card named " + name);
    }
}
