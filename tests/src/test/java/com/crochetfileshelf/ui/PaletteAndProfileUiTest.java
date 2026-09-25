package com.crochetfileshelf.ui;

import com.crochetfileshelf.base.BaseUiTest;
import com.crochetfileshelf.base.TestConfig;
import com.crochetfileshelf.pages.PalettePage;
import com.crochetfileshelf.pages.ProfilePage;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("UI: palette and profile")
public class PaletteAndProfileUiTest extends BaseUiTest {

    @Test
    @DisplayName("Add a color, find it with the search, rename it and delete it")
    void paletteLifecycle() {
        String tag = unique("E2E yarn");
        logInAsFirstUser();
        open("/palette");
        PalettePage palette = new PalettePage(driver);

        palette.addColor(tag + " Rose", "#C08497");
        assertTrue(palette.colors().contains(tag + " Rose"));

        palette.search(tag);
        palette.waitForColors(List.of(tag + " Rose"));

        palette.search("zzz-nothing-" + tag);
        wait.until(d -> palette.noResultsMessageIsShown());

        palette.search(tag);
        palette.waitForColors(List.of(tag + " Rose"));
        palette.editColorName(tag + " Rose", tag + " Dusty");
        assertTrue(palette.colors().contains(tag + " Dusty"));

        palette.deleteColor(tag + " Dusty");
        assertFalse(palette.colors().contains(tag + " Dusty"));
    }

    @Test
    @DisplayName("The profile shows the username and saves a bio")
    void profileEdit() {
        logInAsFirstUser();
        open("/profile");
        ProfilePage profile = new ProfilePage(driver);

        assertTrue(profile.username().contains(TestConfig.USER1_USERNAME));

        String bio = "Bio written by an automated test " + System.nanoTime() % 100000;
        profile.editBio(bio);
        assertEquals(bio, profile.bio());
    }

    @Test
    @DisplayName("The chosen theme is applied and survives a reload")
    void themeSwitch() {
        logInAsFirstUser();
        open("/profile");
        ProfilePage profile = new ProfilePage(driver);

        profile.chooseTheme("Dark");
        wait.until(d -> "dark".equals(profile.activeTheme()));

        driver.navigate().refresh();
        ProfilePage reloaded = new ProfilePage(driver);
        assertEquals("dark", reloaded.activeTheme());

        reloaded.chooseTheme("Light");
        wait.until(d -> "light".equals(reloaded.activeTheme()));
    }
}
