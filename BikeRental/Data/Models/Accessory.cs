namespace BikeRental.Data.Models
{
    /// <summary>
    /// Accessory model for bike attachments and add-ons.
    /// </summary>
    public class Accessory
    {
        public int AccessoryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double UnitPrice { get; set; }
        public int StockCount { get; set; }

        /// <summary>
        /// Valid values: "mountain", "beach", "all".
        /// </summary>
        public string[] CompatibleWith { get; set; } = Array.Empty<string>();
    }
}
