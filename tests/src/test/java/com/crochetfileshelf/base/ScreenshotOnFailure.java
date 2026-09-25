package com.crochetfileshelf.base;

import org.junit.jupiter.api.extension.AfterTestExecutionCallback;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.openqa.selenium.OutputType;
import org.openqa.selenium.TakesScreenshot;
import org.openqa.selenium.WebDriver;

import java.lang.reflect.Field;
import java.nio.file.Files;
import java.nio.file.Path;

/** Saves a screenshot to tests/screenshots when a browser test fails, before the browser is closed. */
public class ScreenshotOnFailure implements AfterTestExecutionCallback {

    @Override
    public void afterTestExecution(ExtensionContext context) {
        if (context.getExecutionException().isEmpty()) {
            return;
        }

        try {
            Object instance = context.getRequiredTestInstance();
            Field field = BaseUiTest.class.getDeclaredField("driver");
            field.setAccessible(true);
            WebDriver driver = (WebDriver) field.get(instance);
            if (!(driver instanceof TakesScreenshot screenshot)) {
                return;
            }

            Path directory = Path.of("screenshots");
            Files.createDirectories(directory);
            Path file = directory.resolve(context.getRequiredTestClass().getSimpleName() + "-"
                    + context.getRequiredTestMethod().getName() + ".png");
            Files.write(file, screenshot.getScreenshotAs(OutputType.BYTES));
            System.out.println("Screenshot saved: " + file.toAbsolutePath());
        } catch (Exception ignored) {
            // a missing screenshot must never hide the real failure
        }
    }
}
