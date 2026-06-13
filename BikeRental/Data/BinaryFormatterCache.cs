using System.IO;
using System.Text;
using System.Text.Json;

namespace BikeRental.Data
{
    // Cache implementation using JSON via System.Text.Json.
    // System.Text.Json is built-in to .NET 5+ and provides high-performance serialization.
    internal static class BinaryFormatterCache
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        // Returns true if the .json sidecar is newer than the source file it was made from.
        public static bool IsFresh(string cachePath, string sourcePath)
        {
            return File.Exists(cachePath)
                && File.GetLastWriteTimeUtc(cachePath) >= File.GetLastWriteTimeUtc(sourcePath);
        }

        // Reads cached data from JSON file and deserializes it back to the original type.
        public static T Read<T>(string cachePath)
        {
            var json = File.ReadAllText(cachePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize cache file: {cachePath}");
        }

        // Serializes data to JSON and writes it to a cache file.
        public static void Write<T>(string cachePath, T data)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(cachePath, json, Encoding.UTF8);
        }
    }
}
