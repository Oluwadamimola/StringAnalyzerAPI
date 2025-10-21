# StringAnalyzerAPI

A RESTful API that analyzes strings and stores their computed properties. This API can calculate string length, check if a string is a palindrome, count unique characters, word count, SHA-256 hash, and character frequency. It also supports filtering and natural language queries.

# Features
* Analyze String – Submit a string to analyze and store its properties.
* Get Specific String – Retrieve a previously analyzed string by its value.
* Get All Strings with Filters – Retrieve strings that match certain criteria (palindrome, length, word count, contains specific character).
* Natural Language Filtering – Query strings using natural language.
* Delete String – Remove a string from the system.

# Setup Instructions
* git clone https://github.com/Oluwadamimola/StringAnalyzerAPI.git
cd StringAnalyzerAPI
* Install dependencies
* Have .NET 8 SDK installed
* Run using dotnet run

# Dependencies
* Microsoft.AspNetCore.App
* .NET 9 SDK
* ASPNET Core
* Swashbuckle.AspNetCore
Install using: dotnet add package Swashbuckle.AspNetCore 


# API Endpoint
* Request body
{
  "value": "string to analyze"
}
Success Response
{
  "id": "sha256_hash_value",
  "value": "string to analyze",
  "properties": {
    "length": 17,
    "is_palindrome": false,
    "unique_characters": 12,
    "word_count": 3,
    "sha256_hash": "abc123...",
    "character_frequency_map": {
      "s": 2,
      "t": 3,
      "r": 2
    }
  },
  "created_at": "2025-08-27T10:00:00Z"
}
Error Responses:
409 Conflict – String already exists
400 Bad Request – Missing or invalid value
422 Unprocessable Entity – Value is not a string

* GET /strings/{string_value}
{
  "id": "sha256_hash_value",
  "value": "requested string",
  "properties": { /* ... */ },
  "created_at": "2025-08-27T10:00:00Z"
}
Error Response:
404 Not Found – String does not exist

* GET /strings?is_palindrome=true&min_length=5&max_length=20&word_count=2&contains_character=a
{
  "data": [
    {
      "id": "hash1",
      "value": "string1",
      "properties": { /* ... */ },
      "created_at": "2025-08-27T10:00:00Z"
    }
  ],
  "count": 15,
  "filters_applied": {
    "is_palindrome": true,
    "min_length": 5,
    "max_length": 20,
    "word_count": 2,
    "contains_character": "a"
  }
}
Error Response:
400 Bad Request – Invalid query parameters

* GET /strings/filter-by-natural-language?query=all%20single%20word%20palindromic%20strings
Example Queries:
"all single word palindromic strings" → word_count=1, is_palindrome=true
"strings longer than 10 characters" → min_length=11

Success Response
{
  "data": [ /* array of matching strings */ ],
  "count": 3,
  "interpreted_query": {
    "original": "all single word palindromic strings",
    "parsed_filters": {
      "word_count": 1,
      "is_palindrome": true
    }
  }
}
Error Responses:
400 Bad Request – Unable to parse query
422 Unprocessable Entity – Conflicting filters

* DELETE /strings/{string_value}

Success Response (204 No Content)
Error Response: 404 Not Found – String does not exist

# Testing the API
You can test using: Swagger UI