package com.crochetfileshelf.api;

import com.crochetfileshelf.base.TestConfig;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static com.crochetfileshelf.api.ApiClient.body;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("API: authentication")
public class AuthApiTest extends ApiTestBase {

    @Test
    @DisplayName("Login returns an access token and the user summary")
    void loginReturnsTokenAndUser() {
        ApiClient.Response response = ApiClient.anonymous().post("/api/auth/login",
                body("email", TestConfig.USER1_EMAIL, "password", TestConfig.USER1_PASSWORD));

        assertEquals(200, response.status());
        assertFalse(response.json().get("accessToken").asText().isBlank());
        assertEquals(TestConfig.USER1_EMAIL, response.json().get("user").get("email").asText());
        assertEquals(TestConfig.USER1_USERNAME, response.json().get("user").get("username").asText());
    }

    @Test
    @DisplayName("Wrong password is rejected with 401 and a message")
    void wrongPasswordIsRejected() {
        ApiClient.Response response = ApiClient.anonymous().post("/api/auth/login",
                body("email", TestConfig.USER1_EMAIL, "password", "definitely-wrong"));

        assertEquals(401, response.status());
        assertFalse(response.message().isBlank());
    }

    @Test
    @DisplayName("Unknown email gets the same answer as a wrong password")
    void unknownEmailLooksLikeWrongPassword() {
        ApiClient.Response wrongPassword = ApiClient.anonymous().post("/api/auth/login",
                body("email", TestConfig.USER1_EMAIL, "password", "definitely-wrong"));
        ApiClient.Response unknownEmail = ApiClient.anonymous().post("/api/auth/login",
                body("email", "nobody-" + System.nanoTime() + "@example.com", "password", "definitely-wrong"));

        assertEquals(401, unknownEmail.status());
        assertEquals(wrongPassword.message(), unknownEmail.message());
    }

    @Test
    @DisplayName("Protected endpoints need a valid token")
    void protectedEndpointsNeedToken() {
        assertEquals(401, ApiClient.anonymous().get("/api/works").status());
        assertEquals(401, ApiClient.anonymous().get("/api/folders").status());
        assertEquals(401, ApiClient.anonymous().withToken("not-a-real-token").get("/api/works").status());
    }

    @Test
    @DisplayName("Registering with an email that already exists is a conflict")
    void registerWithExistingEmailConflicts() {
        ApiClient.Response response = ApiClient.anonymous().post("/api/auth/register", body(
                "email", TestConfig.USER1_EMAIL,
                "password", "Another1234",
                "firstName", "Some",
                "lastName", "One",
                "username", "someone" + System.nanoTime() % 100000));

        assertEquals(409, response.status());
    }

    @Test
    @DisplayName("Registration input is validated on the server")
    void registerValidatesInput() {
        ApiClient.Response response = ApiClient.anonymous().post("/api/auth/register", body(
                "email", "not-an-email",
                "password", "short",
                "firstName", "",
                "lastName", "",
                "username", "a"));

        assertEquals(400, response.status());
    }

    @Test
    @DisplayName("Username availability tells taken from free")
    void usernameAvailability() {
        ApiClient anonymous = ApiClient.anonymous();

        assertFalse(anonymous.get("/api/auth/username-available?username=" + TestConfig.USER1_USERNAME)
                .json().get("available").asBoolean());
        assertFalse(anonymous.get("/api/auth/username-available?username=" + TestConfig.USER1_USERNAME.toUpperCase())
                .json().get("available").asBoolean(), "usernames are case-insensitive");
        assertTrue(anonymous.get("/api/auth/username-available?username=free" + System.nanoTime() % 1000000)
                .json().get("available").asBoolean());
        assertFalse(anonymous.get("/api/auth/username-available?username=no%20spaces")
                .json().get("available").asBoolean());
    }

    @Test
    @DisplayName("Error messages follow the Accept-Language header")
    void errorMessagesAreLocalized() {
        String missingWork = "/api/works/00000000-0000-0000-0000-000000000001";

        String english = alice.withLanguage("en").get(missingWork).message();
        String serbian = alice.withLanguage("sr").get(missingWork).message();
        String german = alice.withLanguage("de").get(missingWork).message();

        assertFalse(english.isBlank());
        assertNotEquals(english, serbian);
        assertNotEquals(english, german);
        assertNotEquals(serbian, german);
    }
}
