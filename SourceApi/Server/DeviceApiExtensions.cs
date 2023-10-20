using SerialPortProxy;

using WebSamDeviceApis.Actions.Device;
using WebSamDeviceApis.Actions.SerialPort;
using WebSamDeviceApis.Actions.Source;
using WebSamDeviceApis.Actions.VeinSource;

namespace WebSamDeviceApis;

public static class Configuration
{
    public static void UseDeviceApi(this IServiceCollection services, IConfiguration configuration)
    {

        switch (configuration["SourceType"])
        {
            case "simulated":
                services.AddSingleton<SimulatedSource>();
                services.AddSingleton<ISource>(x => x.GetRequiredService<SimulatedSource>());
                services.AddSingleton<ISimulatedSource>(x => x.GetRequiredService<SimulatedSource>());
                break;
            case "vein":
                services.AddSingleton<VeinClient>(new VeinClient(new(), "localhost", 8080));
                services.AddSingleton<VeinSource>();
                services.AddSingleton<ISource>(x => x.GetRequiredService<VeinSource>());
                break;
            case "serial":
                services.AddSingleton<ISource, SerialPortSource>();
                break;
            default:
                throw new NotImplementedException($"Unknown SourceType: {configuration["SourceType"]}");
        }

        {
            var portName = configuration["SerialPort:PortName"];
            var mockType = configuration["SerialPort:PortMockType"];

            var config = new SerialPortConfiguration();

            if (!string.IsNullOrEmpty(portName))
            {
                if (!string.IsNullOrEmpty(mockType))
                    throw new NotSupportedException("serial port name and port mock type must not be both set.");

                config.UseMockType = false;
                config.PortNameOrMockType = portName;
            }
            else if (!string.IsNullOrEmpty(mockType))
            {
                config.UseMockType = true;
                config.PortNameOrMockType = mockType;
            }
            else
                throw new NotSupportedException("either serial port name or port mock type must be set.");

            services.AddSingleton(config);
            services.AddSingleton<SerialPortService>();

            services.AddScoped<IDevice, SerialPortDevice>();
        }

    }
}
