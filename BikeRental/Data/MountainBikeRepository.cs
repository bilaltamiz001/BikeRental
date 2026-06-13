using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using BikeRental.Data.Models;

namespace BikeRental.Data
{
    /// <summary>
    /// Mountain bike repository with JSON caching for improved performance.
    /// </summary>
    public class MountainBikeRepository : IMountainBikeRepository
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public MountainBikeRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Gets all mountain bikes.
        /// </summary>
        public List<MountainBike> GetAll()
        {
            try
            {
                // Validate file exists before attempting to load
                if (!File.Exists(_filePath))
                {
                    throw new FileNotFoundException($"Mountain bike data file not found: {_filePath}");
                }

                var cachePath = _filePath + ".cache.json";

                if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                    return BinaryFormatterCache.Read<List<MountainBike>>(cachePath);

                var bikes = IsolatedDataLoader.LoadMountainBikes(_filePath);
                BinaryFormatterCache.Write(cachePath, bikes);
                return bikes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load mountain bikes from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Gets a mountain bike by ID.
        /// </summary>
        public MountainBike? GetById(int id)
        {
            return GetAll().FirstOrDefault(b => b.BikeID == id);
        }

        /// <summary>
        /// Persists mountain bikes to JSON and updates the cache.
        /// </summary>
        public void Save(List<MountainBike> bikes)
        {
            var json = JsonSerializer.Serialize(bikes, _jsonOptions);
            File.WriteAllText(_filePath, json, Encoding.UTF8);

            // Update the cache.
            BinaryFormatterCache.Write(_filePath + ".cache.json", bikes);
        }
    }
}
