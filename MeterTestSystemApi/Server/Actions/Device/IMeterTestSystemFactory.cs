using MeterTestSystemApi.Models.Configuration;

namespace MeterTestSystemApi.Actions.Device;

/// <summary>
/// Singleton factory to create a meter test system.
/// </summary>
public interface IMeterTestSystemFactory
{
    /// <summary>
    /// Create a new meter test system based on the given configuration.
    /// </summary>
    /// <param name="configuration">Configuration to use.</param>
    public void Inititalize(MeterTestSystemConfiguration configuration);

    /// <summary>
    /// Get the meter test system to use - will be available after initialisation.
    /// </summary>
    public IMeterTestSystem MeterTestSystem { get; }
}
