using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SourceApi.Model;

namespace SourceApi.Actions.Source
{
    /// <summary>
    /// Simulatetes the behaviour of a ZERA source.
    /// </summary>
    public class SimulatedSource : ISource, ISimulatedSource
    {
        #region ContructorAndDependencyInjection
        private readonly ILogger<SimulatedSource> _logger;
        private readonly IConfiguration _configuration;
        private readonly SourceCapabilities _sourceCapabilities;
        private readonly LoadpointInfo _info = new();

        /// <summary>
        /// Constructor that injects logger and configuration and uses default source capablities.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="configuration">The configuration o be used.</param>
        public SimulatedSource(ILogger<SimulatedSource> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _sourceCapabilities = new()
            {
                Phases = new() {
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    },
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    },
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    }
                },
                FrequencyRanges = new() {
                    new(40, 60, 0.1, FrequencyMode.SYNTHETIC)
                }
            };
        }

        /// <summary>
        /// Constructor that injects logger, configuration and capabilities.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="configuration">The configuration o be used.</param>
        /// <param name="sourceCapabilities">The capabilities of the source which should be simulated.</param>
        public SimulatedSource(ILogger<SimulatedSource> logger, IConfiguration configuration, SourceCapabilities sourceCapabilities)
        {
            _logger = logger;
            _configuration = configuration;
            _sourceCapabilities = sourceCapabilities;
        }

        #endregion

        private Loadpoint? _loadpoint;
        private SimulatedSourceState? _simulatedSourceState;

        /// <inheritdoc/>
        public Task<SourceResult> SetLoadpoint(Loadpoint loadpoint)
        {
            var isValid = SourceCapabilityValidator.IsValid(loadpoint, _sourceCapabilities);

            if (isValid == SourceResult.SUCCESS)
            {
                _logger.LogTrace("Loadpoint set, source turned on.");
                _loadpoint = loadpoint;
            }

            return Task.FromResult(isValid);
        }

        /// <inheritdoc/>
        public Task<SourceResult> TurnOff()
        {
            _logger.LogTrace("Source turned off.");
            _loadpoint = null;

            return Task.FromResult(SourceResult.SUCCESS);
        }

        /// <inheritdoc/>
        public Loadpoint? GetCurrentLoadpoint()
        {
            return _loadpoint;
        }

        /// <inheritdoc/>
        public void SetSimulatedSourceState(SimulatedSourceState simulatedSourceState)
        {
            _simulatedSourceState = simulatedSourceState;
        }

        /// <inheritdoc/>
        public SimulatedSourceState? GetSimulatedSourceState()
        {
            return _simulatedSourceState;
        }

        public Task<SourceCapabilities> GetCapabilities()
        {
            return Task.FromResult(_sourceCapabilities);
        }

        public Task SetDosageMode(bool on)
        {
            throw new NotImplementedException();
        }

        public Task SetDosageEnergy(double value)
        {
            throw new NotImplementedException();
        }

        public Task StartDosage()
        {
            throw new NotImplementedException();
        }

        public Task CancelDosage()
        {
            throw new NotImplementedException();
        }

        public Task<DosageProgress> GetDosageProgress()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CurrentSwitchedOffForDosage()
        {
            throw new NotImplementedException();
        }

        public LoadpointInfo GetActiveLoadpointInfo() => _info;

        public bool Available => true;
    }
}