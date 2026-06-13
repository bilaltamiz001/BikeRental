namespace BikeRental.Controllers.ApiVersioning
{
    /// <summary>
    /// API version constants for version management and routing.
    /// </summary>
    public static class ApiVersion
    {
        /// <summary>
        /// Version 1.0 of the API.
        /// </summary>
        public const string V1 = "1.0";

        /// <summary>
        /// Version 1.1 of the API with async endpoints.
        /// </summary>
        public const string V1_1 = "1.1";

        /// <summary>
        /// Gets the default API version.
        /// </summary>
        public static string Default => V1_1;
    }

    /// <summary>
    /// API version routing helper.
    /// </summary>
    public static class ApiVersionRouting
    {
        /// <summary>
        /// Gets the versioned route for a controller.
        /// </summary>
        public static string GetVersionedRoute(string version = null)
        {
            version ??= ApiVersion.Default;
            return $"api/v{version}/[controller]";
        }
    }
}
