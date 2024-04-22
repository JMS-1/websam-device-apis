using Microsoft.Extensions.Logging.Abstractions;
using SerialPortProxy;
using SourceApi.Actions.SerialPort;
using SourceApi.Actions.SerialPort.FG30x;
using SourceApi.Actions.Source;

namespace SourceApi.Tests.Actions.SerialPort;

[TestFixture]
public class FGSourceTests
{
    class LoggingSourceMock : SerialPortFGMock
    {
        public readonly List<string> Commands = new();

        public override void WriteLine(string command)
        {
            Commands.Add(command);

            base.WriteLine(command);
        }
    }

    private readonly NullLogger<SerialPortFGSource> _portLogger = new();

    private readonly NullLogger<ISerialPortConnection> _connectionLogger = new();

    private ISerialPortConnection _device;

    private SerialPortFGSource _source;

    private LoggingSourceMock _port;

    [SetUp]
    public void SetUp()
    {
        _port = new();
        _device = SerialPortConnection.FromMockedPortInstance(_port, _connectionLogger);
        _source = new(_portLogger, _device, new CapabilitiesMap(), new SourceCapabilityValidator());

        _source.SetAmplifiers(Model.VoltageAmplifiers.VU220, Model.CurrentAmplifiers.VI220, Model.VoltageAuxiliaries.V210, Model.CurrentAuxiliaries.V200);
    }

    [TearDown]
    public void TearDown()
    {
        _device?.Dispose();
    }

    [TestCase(0.01, "IPAAR000.010000.00S000.020240.00T000.030120.00")]
    [TestCase(0.5, "IPAAR000.500000.00S001.000240.00T001.500120.00")]
    public async Task Can_Set_Valid_Loadpoint(double baseAngle, string current)
    {
        Assert.That(_source.GetCurrentLoadpoint(), Is.Null);

        var result = await _source.SetLoadpoint(new Model.TargetLoadpoint
        {
            Frequency = new()
            {
                Mode = Model.FrequencyMode.SYNTHETIC,
                Value = 50
            },
            Phases = new() {
                new()  {
                    Current = new() { AcComponent = new () {Rms=1 * baseAngle, Angle=0}, On=true},
                    Voltage = new() { AcComponent = new () {Rms=220, Angle=0}, On=true},
                },
                new() {
                    Current = new() { AcComponent = new () {Rms=2 * baseAngle, Angle=120}, On=true},
                    Voltage = new() { AcComponent = new () {Rms=221, Angle=120}, On=false},
                },
                new() {
                    Current = new() { AcComponent = new () {Rms=3 * baseAngle, Angle=240}, On=false},
                    Voltage = new() { AcComponent = new () {Rms=222, Angle=240}, On=true},
                },
            },
            VoltageNeutralConnected = true,
        });

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.SUCCESS));

        Assert.That(_port.Commands, Is.EqualTo(new string[] {
            "FR50.00",
            "UPAER220.000000.00S221.000240.00T222.000120.00",
            current,
            "UIEAEPPAAAA"
        }));

        var loadpoint = _source.GetCurrentLoadpoint();

        Assert.That(loadpoint, Is.Not.Null);
        Assert.That(loadpoint.Frequency.Value, Is.EqualTo(50));
    }

    [Test]
    public async Task Can_Set_IEC_Loadpoint()
    {
        Assert.That(_source.GetCurrentLoadpoint(), Is.Null);

        var result = await _source.SetLoadpoint(new Model.TargetLoadpoint
        {
            Frequency = new()
            {
                Mode = Model.FrequencyMode.SYNTHETIC,
                Value = 50
            },
            Phases = new() {
                new()  {
                    Current = new() { AcComponent = new () { Rms=10, Angle=0}, On=true},
                    Voltage = new() { AcComponent = new () { Rms=120, Angle=330}, On=true},
                },
                new() {
                    Current = new() { AcComponent = new () { Rms=10, Angle=240}, On=true},
                    Voltage = new() { AcComponent = new () { Rms=120, Angle=210}, On=true},
                },
                new() {
                    Current = new() { AcComponent = new () { Rms=10, Angle=120}, On=true},
                    Voltage = new() { AcComponent = new () { Rms=120, Angle=90}, On=true},
                },
            },
            VoltageNeutralConnected = true,
        });

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.SUCCESS));

        Assert.That(_port.Commands, Is.EqualTo(new string[] {
            "FR50.00",
            "UPAER120.000000.00S120.000120.00T120.000240.00",
            "IPAAR010.000330.00S010.000090.00T010.000210.00",
            "UIEEEPPPAAA"
        }));

        var loadpoint = _source.GetCurrentLoadpoint();

        Assert.That(loadpoint, Is.Not.Null);
        Assert.That(loadpoint.Frequency.Value, Is.EqualTo(50));
    }
}
