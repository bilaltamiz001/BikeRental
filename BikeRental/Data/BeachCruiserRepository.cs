using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BikeRental.Data.Models;

namespace BikeRental.Data
{
    /// <summary>
    /// Beach cruiser repository with JSON caching for XML data persistence.
    /// Maintains XML file format for storage while using JSON cache for fast reads.
    /// </summary>
    public class BeachCruiserRepository : IBeachCruiserRepository
    {
        private readonly string _filePath;

        public BeachCruiserRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Gets all beach cruisers.
        /// </summary>
        public List<BeachCruiser> GetAll()
        {
            try
            {
                // Validate file exists before attempting to load
                if (!File.Exists(_filePath))
                {
                    throw new FileNotFoundException($"Beach cruiser data file not found: {_filePath}");
                }

                var cachePath = _filePath + ".cache.json";

                if (BinaryFormatterCache.IsFresh(cachePath, _filePath))
                    return BinaryFormatterCache.Read<List<BeachCruiser>>(cachePath);

                var bikes = IsolatedDataLoader.LoadBeachCruisers(_filePath);
                BinaryFormatterCache.Write(cachePath, bikes);
                return bikes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load beach cruisers from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Gets a beach cruiser by ID.
        /// </summary>
        public BeachCruiser? GetById(int id)
        {
            return GetAll().FirstOrDefault(b => b.bike_id == id);
        }

        /// <summary>
        /// Persists beach cruisers back to XML format.
        /// </summary>
        public void Save(List<BeachCruiser> bikes)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("beach_cruisers",
                    bikes.Select(b => new XElement("bike",
                        new XElement("bike_id",       b.bike_id),
                        new XElement("bike_name",     b.bike_name ?? string.Empty),
                        new XElement("color",         b.color ?? string.Empty),
                        new XElement("frame_size",    b.frame_size ?? string.Empty),
                        new XElement("price_per_day", b.price_per_day.ToString(CultureInfo.InvariantCulture)),
                        new XElement("available",     b.available.ToString().ToLower()),
                        new XElement("description",   b.description ?? string.Empty)
                    ))
                )
            );
            doc.Save(_filePath);

            // Update the cache.
            BinaryFormatterCache.Write(_filePath + ".cache.json", bikes);
        }
    }
}

