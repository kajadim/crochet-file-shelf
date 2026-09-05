# Backend (ASP.NET Core Web API) - image za povremeno pokretanje cele aplikacije preko Docker-a.
# Za svakodnevni razvoj i dalje se koristi Visual Studio (F5, debugging) - ovo je samo "brza provera".

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]
