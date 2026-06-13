using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using BikeRental.Data.Models;

namespace BikeRental.Data
{
    /// <summary>
    /// Accessory repository with JSON caching for fast access.
    /// </summary>
    public class AccessoryRepository : IAccessoryRepository
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public AccessoryRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Gets all accessories.
        /// </summary>
        public List<Accessory> GetAll()
        {
            var cachePath = _filePath + ".cache.json";

            if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                return BinaryFormatterCache.Read<List<Accessory>>(cachePath);

            var accessories = IsolatedDataLoader.LoadAccessories(_filePath);
            BinaryFormatterCache.Write(cachePath, accessories);
            return accessories;
        }

        /// <summary>
        /// Gets an accessory by ID.
        /// </summary>
        public Accessory? GetById(int id)
        {
            return GetAll().FirstOrDefault(a => a.AccessoryID == id);
        }

        /// <summary>
        /// Writes accessories to disk as JSON and updates the cache.
        /// </summary>
        public void Save(List<Accessory> accessories)
        {
            var json = JsonSerializer.Serialize(accessories, _jsonOptions);
            File.WriteAllText(_filePath, json, Encoding.UTF8);

            // Keep the cache honest.
            BinaryFormatterCache.Write(_filePath + ".cache.json", accessories);
        }
    }
}
