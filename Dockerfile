# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.sln .
COPY StringAnalyzerAPI/*.csproj ./StringAnalyzerAPI/
RUN dotnet restore StringAnalyzerAPI/StringAnalyzerAPI.csproj

# Copy the remaining source code and build the app
COPY . .
RUN dotnet publish StringAnalyzerAPI/StringAnalyzerAPI.csproj -c Release -o /out

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "StringAnalyzerAPI.dll"]

