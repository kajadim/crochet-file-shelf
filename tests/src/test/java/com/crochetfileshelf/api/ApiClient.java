package com.crochetfileshelf.api;

import com.crochetfileshelf.base.TestConfig;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.LinkedHashMap;
import java.util.Map;

/**
 * A very small HTTP client for the backend REST API. It talks to the same address the browser uses
 * (the Angular dev server proxies /api to the backend), so no extra address has to be configured.
 */
public class ApiClient {

    public record Response(int status, JsonNode json, String raw) {

        public String message() {
            return json != null && json.has("message") ? json.get("message").asText() : "";
        }

        public String id() {
            return json != null && json.has("id") ? json.get("id").asText() : null;
        }

        public boolean isOk() {
            return status >= 200 && status < 300;
        }
    }

    private static final ObjectMapper MAPPER = new ObjectMapper();

    private final HttpClient http = HttpClient.newBuilder().version(HttpClient.Version.HTTP_1_1).connectTimeout(Duration.ofSeconds(10)).build();
    private static final String RESOLVED_BASE_URL = resolveReachableBaseUrl();

    private final String baseUrl = RESOLVED_BASE_URL;
    private String token;
    private String language;

    /**
     * 'localhost' can mean IPv4 or IPv6 and a dev server usually listens on only one of them, while Java
     * tries just the first address. So the address that actually accepts connections is picked here.
     */
    private static String resolveReachableBaseUrl() {
        URI uri = URI.create(TestConfig.BASE_URL);
        int port = uri.getPort() > 0 ? uri.getPort() : ("https".equals(uri.getScheme()) ? 443 : 80);
        try {
            for (java.net.InetAddress address : java.net.InetAddress.getAllByName(uri.getHost())) {
                try (java.net.Socket socket = new java.net.Socket()) {
                    socket.connect(new java.net.InetSocketAddress(address, port), 700);
                    String host = address instanceof java.net.Inet6Address ? "[" + address.getHostAddress().replaceAll("%.*", "") + "]" : address.getHostAddress();
                    return uri.getScheme() + "://" + host + ":" + port;
                } catch (IOException ignored) {
                    // try the next address
                }
            }
        } catch (IOException ignored) {
            // fall through to the configured address
        }
        return TestConfig.BASE_URL;
    }

    public static ApiClient anonymous() {
        return new ApiClient();
    }

    public static ApiClient loginAs(String email, String password) {
        ApiClient client = new ApiClient();
        Response response = client.post("/api/auth/login", Map.of("email", email, "password", password));
        if (!response.isOk()) {
            throw new IllegalStateException(
                    "Could not log in as " + email + " (status " + response.status() + "): " + response.raw());
        }
        client.token = response.json().get("accessToken").asText();
        client.userId = response.json().get("user").get("id").asText();
        return client;
    }

    private String userId;

    public String userId() {
        return userId;
    }

    public ApiClient withToken(String value) {
        this.token = value;
        return this;
    }

    public ApiClient withLanguage(String value) {
        this.language = value;
        return this;
    }

    public Response get(String path) {
        return send("GET", path, null);
    }

    public Response post(String path, Object body) {
        return send("POST", path, body);
    }

    public Response put(String path, Object body) {
        return send("PUT", path, body);
    }

    public Response delete(String path) {
        return send("DELETE", path, null);
    }

    public Response send(String method, String path, Object body) {
        try {
            HttpRequest.Builder builder = HttpRequest.newBuilder(URI.create(baseUrl + path))
                    .timeout(Duration.ofSeconds(30))
                    .header("Accept", "application/json");

            if (token != null) {
                builder.header("Authorization", "Bearer " + token);
            }
            if (language != null) {
                builder.header("Accept-Language", language);
            }

            HttpRequest.BodyPublisher publisher = HttpRequest.BodyPublishers.noBody();
            if (body != null) {
                builder.header("Content-Type", "application/json");
                publisher = HttpRequest.BodyPublishers.ofString(MAPPER.writeValueAsString(body));
            }

            HttpResponse<String> response = http.send(builder.method(method, publisher).build(),
                    HttpResponse.BodyHandlers.ofString());

            String raw = response.body() == null ? "" : response.body();
            JsonNode json = null;
            if (!raw.isBlank()) {
                try {
                    json = MAPPER.readTree(raw);
                } catch (IOException ignored) {
                    json = null;
                }
            }
            return new Response(response.statusCode(), json, raw);
        } catch (IOException | InterruptedException e) {
            if (e instanceof InterruptedException) {
                Thread.currentThread().interrupt();
            }
            throw new IllegalStateException("Request " + method + " " + path + " failed", e);
        }
    }

    public static Map<String, Object> body(Object... keyValues) {
        Map<String, Object> map = new LinkedHashMap<>();
        for (int i = 0; i < keyValues.length; i += 2) {
            map.put((String) keyValues[i], keyValues[i + 1]);
        }
        return map;
    }
}
