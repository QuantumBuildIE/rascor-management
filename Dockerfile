FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY *.sln ./
COPY src/ ./src/

# Restore
RUN dotnet restore src/Rascor.API/Rascor.API.csproj

# Build and publish
RUN dotnet publish src/Rascor.API/Rascor.API.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Rascor.API.dll"]
