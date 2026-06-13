namespace BikeRental.Data.Models
{
    /// <summary>
    /// Result of an accessory order request.
    /// </summary>
    public class AccessoryRequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public double TotalPrice { get; set; }
        public double DiscountAmount { get; set; }
        public bool BundleDiscountApplied { get; set; }
    }
}
