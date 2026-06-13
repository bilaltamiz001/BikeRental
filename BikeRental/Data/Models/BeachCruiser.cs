namespace BikeRental.Data.Models
{
    /// <summary>
    /// Beach cruiser bike model with relaxed naming conventions (snake_case).
    /// </summary>
    public class BeachCruiser
    {
        public int bike_id { get; set; }
        public string bike_name { get; set; } = string.Empty;
        public string color { get; set; } = string.Empty;
        public string frame_size { get; set; } = string.Empty;
        public decimal price_per_day { get; set; }
        public bool available { get; set; }
        public string description { get; set; } = string.Empty;
    }
}
