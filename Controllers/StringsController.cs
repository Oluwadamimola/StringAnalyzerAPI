using Microsoft.AspNetCore.Mvc;
using StringAnalyzerAPI.Models;
using StringAnalyzerAPI.Services;

namespace StringAnalyzerAPI.Controllers;

[ApiController]
[Route("[Controller]")] 

public class StringsController : ControllerBase
{
    private readonly StringAnalysisService _stringAnalysisService;

    public StringsController(StringAnalysisService stringAnalysisService)
    {
        _stringAnalysisService = stringAnalysisService;
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeString([FromBody] CreateStringRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Value))
            return BadRequest(new { error = "Invalid request body or missing 'value' field." });

        if (request.Value.GetType() != typeof(string))
            return UnprocessableEntity(new { error = "Invalid data type for 'value'. It must be a string." });

        try
        {
            var analyzedString = await _stringAnalysisService.AnalyzeAndStoreStringAsync(request.Value);

            // 409 - String already exists
            if (analyzedString == null)
                return Conflict(new { error = "String already exists in the system." });

            
            return StatusCode(201, new
            {
                id = analyzedString.Id,
                value = analyzedString.Value,
                properties = analyzedString.Properties,
                created_at = analyzedString.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    [HttpGet("{string_value}")]
    public IActionResult GetStringByValue(string string_value)
    {
        var analyzedString = _stringAnalysisService.GetString(string_value);
        if (analyzedString == null)
            return NotFound(new { error = "String does not exist in the system." });

        return Ok(new
        {
            id = analyzedString.Id,
            value = analyzedString.Value,
            properties = analyzedString.Properties,
            created_at = analyzedString.CreatedAt
        });
    }
    [HttpGet]
    public IActionResult GetAllStringsWithFilter(
        [FromQuery] bool? is_palindrome,
        [FromQuery] int? min_length,
        [FromQuery] int? max_length,
        [FromQuery] int? word_count,
        [FromQuery] string? contains_character)
    {
        try
        {
            var results = _stringAnalysisService.GetAllStringsWithFilter(
                is_palindrome, min_length, max_length, word_count, contains_character
            );

            var filtersApplied = new
            {
                is_palindrome, min_length, max_length, word_count, contains_character
            };

            return Ok(new
            {
                data = results.Select(s => new
                {
                    id = s.Id,
                    value = s.Value,
                    properties = s.Properties,
                    created_at = s.CreatedAt
                }),
                count = results.Count(),
                filters_applied = filtersApplied
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("filter-by-natural-language")]
    public IActionResult FilterByNaturalLanguage([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query cannot be empty." });

        try
        {
            
            var result = _stringAnalysisService.FilterByNaturalLanguage(query);

            var (matchingStrings, parsedFilters) = result; 

            if (matchingStrings == null || !matchingStrings.Any())
                return UnprocessableEntity(new { error = "Query parsed but resulted in conflicting filters." });

            return Ok(new
            {
                data = matchingStrings.Select(s => new
                {
                    id = s.Id,
                    value = s.Value,
                    properties = s.Properties,
                    created_at = s.CreatedAt
                }),
                count = matchingStrings.Count(),
                interpreted_query = new
                {
                    original = query,
                    parsed_filters = parsedFilters
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpDelete("{string_value}")]
    public IActionResult DeleteString(string string_value)
    {
        if (string.IsNullOrWhiteSpace(string_value))
            return BadRequest(new { error = "String value cannot be empty." });

        var deleted = _stringAnalysisService.DeleteString(string_value);

        if (!deleted)
            return NotFound(new { error = "String does not exist in the system." });

        return NoContent(); 
    }
}
