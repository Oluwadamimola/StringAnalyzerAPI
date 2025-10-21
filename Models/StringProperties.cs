namespace StringAnalyzerAPI.Models
{
    public class StringProperties
    {
        public int Length { get; set; }
        public bool IsPalindrome { get; set; }
        public int UniqueCharacters { get; set; }
        public int WordCount { get; set; }
        public string? Sas256Hash { get; set; }
        public Dictionary<char, int>? CharacterFrequencyMap { get; set; }
    }
}