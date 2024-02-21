namespace SharedLibrary.Models;

/// <summary>
/// 
/// </summary>
public static class DatabaseCategories
{
    /// <summary>
    /// Startup database esp. holding the configuration of other databases.
    /// </summary>
    public const string Master = "master";

    /// <summary>
    /// Overall configuration.
    /// </summary>
    public const string Configuration = "config";

    /// <summary>
    /// Meter dependant information - e.g. type definitions.
    /// </summary>
    public const string Meter = "meter";

    /// <summary>
    /// Test sequences - e.g. script definitions.
    /// </summary>
    public const string Sequences = "sequences";

    /// <summary>
    /// Test results - e.g. logging.
    /// </summary>
    public const string Results = "results";
}