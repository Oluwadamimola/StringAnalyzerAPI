# Use .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies first (better caching)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the project
COPY . ./
RUN dotnet publish -c Release -o out

# Use runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Railway expects this port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "DynamicProfileAPI.dll"]
