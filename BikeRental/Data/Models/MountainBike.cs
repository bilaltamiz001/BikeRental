namespace BikeRental.Data.Models
{
    /// <summary>
    /// Mountain bike model with PascalCase naming (formal and demanding, like mountains).
    /// </summary>
    public class MountainBike
    {
        public int BikeID { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int GearCount { get; set; }
        public string SuspensionType { get; set; } = string.Empty;
        public string FrameMaterial { get; set; } = string.Empty;
        public double DailyRate { get; set; }
        public bool IsAvailable { get; set; }
        public string Terrain { get; set; } = string.Empty;
        public double WeightKg { get; set; }
    }
}
