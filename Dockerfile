# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY MyBlazorApp/MyBlazorApp.csproj MyBlazorApp/
RUN dotnet restore MyBlazorApp/MyBlazorApp.csproj

# Copy everything else and build
COPY MyBlazorApp/ MyBlazorApp/
WORKDIR /src/MyBlazorApp
RUN dotnet build MyBlazorApp.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish MyBlazorApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables for containerized runtime
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ApiSettings__BaseUrl=http://localhost:8080

ENTRYPOINT ["dotnet", "MyBlazorApp.dll"]
