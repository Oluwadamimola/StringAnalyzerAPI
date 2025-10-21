# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the app
RUN dotnet publish -c Release -o out

# Use the smaller runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the default port (Railway will set its own PORT env var)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "StringAnalyzerAPI.dll"]


