# Runs the Java tests (Maven, JUnit 5, Selenium). The browser is not in this image: the tests drive a
# separate Selenium container (see docker-compose.yml, profile "test"), so no browser has to be installed here.

FROM maven:3.9-eclipse-temurin-17

WORKDIR /tests

# Download dependencies first so that this layer is cached until pom.xml changes.
COPY pom.xml .
RUN mvn -q -B dependency:go-offline

COPY src ./src

CMD ["mvn", "-B", "test"]
