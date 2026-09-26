package com.crochetfileshelf.base;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

public final class TestConfig {

    public static final String BASE_URL;
    public static final String BROWSER;
    public static final boolean HEADLESS;
    public static final String REMOTE_URL;

    public static final String USER1_EMAIL;
    public static final String USER1_PASSWORD;
    public static final String USER1_USERNAME;
    public static final String USER2_EMAIL;
    public static final String USER2_PASSWORD;
    public static final String USER2_USERNAME;
    public static final String USER3_EMAIL;
    public static final String USER3_PASSWORD;

    static {
        Properties props = new Properties();
        try (InputStream in = TestConfig.class.getClassLoader().getResourceAsStream("test.properties")) {
            if (in != null) {
                props.load(in);
            }
        } catch (IOException e) {
            throw new IllegalStateException("Failed to load test.properties", e);
        }

        BASE_URL = read(props, "base.url", "http://localhost:4200");
        BROWSER = read(props, "browser", "chrome").toLowerCase();
        HEADLESS = Boolean.parseBoolean(read(props, "headless", "false"));
        REMOTE_URL = read(props, "remote.url", "");

        USER1_EMAIL = required(props, "user1.email");
        USER1_PASSWORD = required(props, "user1.password");
        USER1_USERNAME = required(props, "user1.username");
        USER2_EMAIL = required(props, "user2.email");
        USER2_PASSWORD = required(props, "user2.password");
        USER2_USERNAME = required(props, "user2.username");
        USER3_EMAIL = read(props, "user3.email", "");
        USER3_PASSWORD = read(props, "user3.password", "");
    }

    private TestConfig() {
    }

    private static String read(Properties props, String key, String fallback) {
        String fromEnv = System.getenv(key.toUpperCase().replace('.', '_'));
        if (fromEnv != null && !fromEnv.isBlank()) {
            return fromEnv;
        }
        return props.getProperty(key, fallback).trim();
    }

    private static String required(Properties props, String key) {
        String value = read(props, key, "");
        if (value.isEmpty()) {
            throw new IllegalStateException("Missing required setting '" + key + "'. Set it in src/test/resources/test.properties "
                    + "(copy test.properties.example) or as the environment variable " + key.toUpperCase().replace('.', '_') + ".");
        }
        return value;
    }
}
