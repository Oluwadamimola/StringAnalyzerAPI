using System.Security.Cryptography;
using System.Text;
using StringAnalyzerAPI.Models;
using System.Text.RegularExpressions;

namespace StringAnalyzerAPI.Services
{
    public class StringAnalysisService
    {
        private readonly List<AnalyzedString> _storage = new();

        // ✅ CREATE / ANALYZE STRING
        public async Task<AnalyzedString?> AnalyzeAndStoreStringAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty");

        var hash = ComputeSha256Hash(value);

        if (_storage.Any(s => s.Id == hash))
            return null; // conflict, already exists

        var properties = new StringProperties
        {
            Length = value.Length,
            IsPalindrome = IsPalindrome(value),
            UniqueCharacters = value.Distinct().Count(),
            WordCount = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            Sas256Hash = hash,
            CharacterFrequencyMap = GetCharacterFrequency(value)
        };

        var analyzedString = new AnalyzedString
        {
            Id = hash,
            Value = value,
            Properties = properties,
            CreatedAt = DateTime.UtcNow
        };

        _storage.Add(analyzedString);

        return await Task.FromResult(analyzedString);
    }


        public AnalyzedString? GetString(string value)
        {
            var hash = ComputeSha256Hash(value);
            return _storage.FirstOrDefault(s => s.Id == hash);
        }

        public IEnumerable<AnalyzedString> GetAllStringsWithFilter(
            bool? isPalindrome = null,
            int? minLength = null,
            int? maxLength = null,
            int? wordCount = null,
            string? containsCharacter = null)
        {
            var query = _storage.AsEnumerable();

            if (isPalindrome.HasValue)
                query = query.Where(s => s.Properties != null && s.Properties.IsPalindrome == isPalindrome.Value);

            if (minLength.HasValue)
                query = query.Where(s => s.Properties != null && s.Properties.Length >= minLength.Value);

            if (maxLength.HasValue)
                query = query.Where(s => s.Properties != null && s.Properties.Length <= maxLength.Value);

            if (wordCount.HasValue)
                query = query.Where(s => s.Properties != null && s.Properties.WordCount == wordCount.Value);
            if (!string.IsNullOrWhiteSpace(containsCharacter))
                query = query.Where(s => s.Value != null && s.Value.Contains(containsCharacter, StringComparison.OrdinalIgnoreCase));


            return query.ToList();
        }

        public (IEnumerable<AnalyzedString> Results, object ParsedQuery) FilterByNaturalLanguage(string query)
        {
            var parsedFilters = new Dictionary<string, object>();

            query = query.ToLower();

            bool? isPalindrome = null;
            int? wordCount = null;
            int? minLength = null;
            string? containsCharacter = null;

            if (query.Contains("palindromic"))
                isPalindrome = true;

            var wordCountMatch = Regex.Match(query, @"single\sword|(\d+)\sword");
            if (wordCountMatch.Success)
            {
                if (wordCountMatch.Value.Contains("single"))
                    wordCount = 1;
                else if (int.TryParse(wordCountMatch.Groups[1].Value, out int wc))
                    wordCount = wc;
            }

            var longerThanMatch = Regex.Match(query, @"longer\s+than\s+(\d+)");
            if (longerThanMatch.Success && int.TryParse(longerThanMatch.Groups[1].Value, out int len))
                minLength = len + 1;

            var containsMatch = Regex.Match(query, @"letter\s+([a-z])");
            if (containsMatch.Success)
                containsCharacter = containsMatch.Groups[1].Value;

            var filtered = GetAllStringsWithFilter(isPalindrome, minLength, null, wordCount, containsCharacter);

            parsedFilters["original"] = query;
            parsedFilters["parsed_filters"] = new
            {
                is_palindrome = isPalindrome,
                word_count = wordCount,
                min_length = minLength,
                contains_character = containsCharacter
            };

            return (filtered, parsedFilters);
        }

        // ✅ DELETE STRING
        public bool DeleteString(string value)
        {
            var hash = ComputeSha256Hash(value);
            var item = _storage.FirstOrDefault(s => s.Id == hash);

            if (item == null) return false;

            _storage.Remove(item);
            return true;
        }

        // 🔹 HELPER METHODS BELOW 🔹

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static bool IsPalindrome(string input)
        {
            var cleaned = new string(input.ToLower().Where(char.IsLetterOrDigit).ToArray());
            return cleaned.SequenceEqual(cleaned.Reverse());
        }

        private static Dictionary<char, int> GetCharacterFrequency(string input)
        {
            var frequency = new Dictionary<char, int>();
            foreach (var c in input)
            {
                if (frequency.ContainsKey(c))
                    frequency[c]++;
                else
                    frequency[c] = 1;
            }
            return frequency;
        }
    }
}