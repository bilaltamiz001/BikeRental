using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using BikeRental.Data.Models;

namespace BikeRental.Data
{
    // Loads data files directly without AppDomain isolation.
    // AppDomain was removed in .NET 5. Direct loading is simpler, faster, and sufficient
    // for this application's data loading needs. The files are read-only during parsing.
    internal static class IsolatedDataLoader
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Reads and parses beach cruiser XML data directly.
        public static List<BeachCruiser> LoadBeachCruisers(string filePath)
        {
            var bikes = new List<BeachCruiser>();
            var doc = XDocument.Load(filePath);

            foreach (var el in doc.Root?.Elements("bike") ?? Enumerable.Empty<XElement>())
            {
                bikes.Add(new BeachCruiser
                {
                    bike_id       = int.Parse(el.Element("bike_id")?.Value ?? "0"),
                    bike_name     = el.Element("bike_name")?.Value ?? string.Empty,
                    color         = el.Element("color")?.Value ?? string.Empty,
                    frame_size    = el.Element("frame_size")?.Value ?? string.Empty,
                    price_per_day = decimal.Parse(el.Element("price_per_day")?.Value ?? "0", CultureInfo.InvariantCulture),
                    available     = bool.Parse(el.Element("available")?.Value ?? "false"),
                    description   = el.Element("description")?.Value ?? string.Empty
                });
            }

            return bikes;
        }

        // Reads and parses mountain bike JSON data directly.
        public static List<MountainBike> LoadMountainBikes(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<MountainBike>>(json, _jsonOptions)
                ?? new List<MountainBike>();
        }

        // Reads and parses accessory JSON data directly.
        public static List<Accessory> LoadAccessories(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Accessory>>(json, _jsonOptions)
                ?? new List<Accessory>();
        }
    }
}
