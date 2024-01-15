using SourceApi.Model;

namespace SourceApi.Actions.SerialPort;

/// <summary>
/// 
/// </summary>
public interface ICapabilitiesMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    SourceCapabilities GetCapabilitiesByModel(string modelName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="voltageAmplifier"></param>
    /// <param name="currentAmplifier"></param>
    /// <returns></returns>
    public SourceCapabilities GetCapabilitiesByAmplifiers(VoltageAmplifiers voltageAmplifier, CurrentAmplifiers currentAmplifier);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="voltageAmplifier"></param>
    /// <returns></returns>
    public double[] GetRangesByAmplifier(VoltageAmplifiers voltageAmplifier);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentAmplifier"></param>
    /// <returns></returns>
    public double[] GetRangesByAmplifier(CurrentAmplifiers currentAmplifier);
}

/// <summary>
/// 
/// </summary>
public class CapabilitiesMap : ICapabilitiesMap
{
    /// <inheritdoc/>
    public SourceCapabilities GetCapabilitiesByModel(string modelName) => GetCapabilitiesByAmplifiers(modelName, modelName);

    /// <inheritdoc/>
    public SourceCapabilities GetCapabilitiesByAmplifiers(VoltageAmplifiers voltageAmplifier, CurrentAmplifiers currentAmplifier) =>
        GetCapabilitiesByAmplifiers(GetVoltageAmplifierKey(voltageAmplifier), GetCurrentAmplifierKey(currentAmplifier));


    private static string GetVoltageAmplifierKey(VoltageAmplifiers amplifier)
    {
        switch (amplifier)
        {
            case VoltageAmplifiers.VU211x0:
            case VoltageAmplifiers.VU211x1:
            case VoltageAmplifiers.VU211x2:
                return "VU211";
            case VoltageAmplifiers.VU221x0:
            case VoltageAmplifiers.VU221x1:
            case VoltageAmplifiers.VU221x2:
            case VoltageAmplifiers.VU221x3:
            case VoltageAmplifiers.VU221x0x2:
            case VoltageAmplifiers.VU221x0x3:
                return "VU221";
            case VoltageAmplifiers.VU220:
            case VoltageAmplifiers.VU220x01:
            case VoltageAmplifiers.VU220x02:
            case VoltageAmplifiers.VU220x03:
            case VoltageAmplifiers.VU220x04:
                return "VU220";
            case VoltageAmplifiers.SVG3020:
                return "SVG3020";
            case VoltageAmplifiers.VUI301:
            case VoltageAmplifiers.VUI302:
                return "VUI302";
            default:
                throw new NotSupportedException($"Unknown voltage amplifier {amplifier}");
        }
    }

    private static string GetCurrentAmplifierKey(CurrentAmplifiers amplifier)
    {
        switch (amplifier)
        {
            case CurrentAmplifiers.VI201x0:
            case CurrentAmplifiers.VI201x0x1:
            case CurrentAmplifiers.VI201x1:
                return "VI201";
            case CurrentAmplifiers.VI202x0:
            case CurrentAmplifiers.VI202x0x1:
            case CurrentAmplifiers.VI202x0x2:
            case CurrentAmplifiers.VI202x0x5:
                return "VI202";
            case CurrentAmplifiers.VI221x0:
                return "VI221";
            case CurrentAmplifiers.VI220:
                return "VI220";
            case CurrentAmplifiers.VI222x0:
            case CurrentAmplifiers.VI222x0x1:
                return "VI222";
            case CurrentAmplifiers.SCG1020:
                return "SCG1020";
            case CurrentAmplifiers.VUI301:
            case CurrentAmplifiers.VUI302:
                return "VUI302";
            default:
                throw new NotSupportedException($"Unknown current amplifier {amplifier}");
        }
    }

    private static SourceCapabilities GetCapabilitiesByAmplifiers(string voltageAmplifier, string currentAmplifier)
    {
        /* See if there are configurations for the amplifiers. */
        if (!VoltageByAmplifier.TryGetValue(voltageAmplifier, out var voltageInfo))
            throw new ArgumentException($"unknown voltage amplifier {voltageAmplifier}", nameof(voltageAmplifier));

        if (!CurrentByAmplifier.TryGetValue(currentAmplifier, out var currentInfo))
            throw new ArgumentException($"unknown current amplifier {currentAmplifier}", nameof(currentAmplifier));

        var (current, _) = currentInfo;
        var (voltage, _) = voltageInfo;

        var capabilties = new SourceCapabilities();

        /* Current configuration uses exactly one phase mapped to three identical configurations. */
        if (current.Phases.Count != 1 || voltage.Phases.Count != 1)
            throw new InvalidOperationException("data mismatch - expected equal number of phases");

        for (var count = 3; count-- > 0;)
            capabilties.Phases.Add(new()
            {
                Current = current.Phases[0].Current,
                Voltage = voltage.Phases[0].Voltage
            });

        /* There may be only one frequency configuration with the same generator mode. */
        if (current.FrequencyRanges.Count != 1 || voltage.FrequencyRanges.Count != 1)
            throw new InvalidOperationException("data mismatch - expected one frequency range");

        if (current.FrequencyRanges[0].Mode != voltage.FrequencyRanges[0].Mode)
            throw new InvalidOperationException("data mismatch - expected same frequency mode");

        capabilties.FrequencyRanges.Add(new()
        {
            Min = Math.Max(current.FrequencyRanges[0].Min, voltage.FrequencyRanges[0].Min),
            Max = Math.Min(current.FrequencyRanges[0].Max, voltage.FrequencyRanges[0].Max),
            Mode = current.FrequencyRanges[0].Mode,
            PrecisionStepSize = Math.Max(current.FrequencyRanges[0].Min, voltage.FrequencyRanges[0].Max),
        });

        return capabilties;
    }

    public double[] GetRangesByAmplifier(VoltageAmplifiers voltageAmplifier)
    {
        if (!VoltageByAmplifier.TryGetValue(GetVoltageAmplifierKey(voltageAmplifier), out var voltageInfo))
            throw new ArgumentException($"unknown voltage amplifier {voltageAmplifier}", nameof(voltageAmplifier));

        return voltageInfo.Item2.Order().ToArray();
    }

    public double[] GetRangesByAmplifier(CurrentAmplifiers currentAmplifier)
    {
        if (!CurrentByAmplifier.TryGetValue(GetCurrentAmplifierKey(currentAmplifier), out var currentInfo))
            throw new ArgumentException($"unknown current amplifier {currentAmplifier}", nameof(currentAmplifier));

        return currentInfo.Item2.Order().ToArray();
    }

    private static readonly Dictionary<string, Tuple<SourceCapabilities, double[]>> VoltageByAmplifier = new() {
    { "MT786",  Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(45, 65, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(20, 500, 0.001) }],
    }, [ 5d, 60d, 125d, 250d, 420d ] )},
    { "VU211", Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(30, 480, 0.001) }],
    }, [ 5d, 60d, 125d, 250d, 420d ]) },
    { "VU220",Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(30, 320, 0.001) }],
    }, [ 5d, 60d, 125d, 250d ]) },
    { "VU221", Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(30, 320, 0.001) }],
    }, [ 5d, 60d, 125d, 250d ]) },
    { "VUI302", Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(30, 320, 0.001) }],
    }, [ 5d, 60d, 125d, 250d ] )},
    { "SVG3020",Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Voltage = new(30, 600, 0.001) }],
    }, [ 5d, 60d, 125d, 250d, 420d ] )} };

    private static readonly Dictionary<string, Tuple<SourceCapabilities, double[]>> CurrentByAmplifier = new() {
    { "MT786", Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(45, 65, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(0.001, 120, 0.001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )},
    { "VI201",Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(500E-6, 160, 0.0001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )},
    { "VI202", Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(500E-6, 120, 0.0001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )},
    { "VI220",Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(500E-6, 120, 0.0001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )},
    { "VI221", Tuple.Create<SourceCapabilities,double[]>(new ()  {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(500E-6, 120, 0.0001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )},
    { "VI222", Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(500E-6, 120, 0.0001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ]) },
    { "VUI302", Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(40, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(12E-3, 120, 0.001) }],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ])},
    { "SCG1020", Tuple.Create<SourceCapabilities,double[]>(new () {
        FrequencyRanges = [new(15, 70, 0.01, FrequencyMode.SYNTHETIC)],
        Phases = [new() { Current = new(0.001, 120, 0.0001)}],
    }, [ 0.02d, 0.05d, 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d ] )} };
}
