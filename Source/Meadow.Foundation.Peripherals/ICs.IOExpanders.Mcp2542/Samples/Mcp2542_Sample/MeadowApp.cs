using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System.Threading.Tasks;

namespace Sensors.Temperature.Mcp2542_Sample;

public class MeadowApp : App<F7CoreComputeV2>
{
    //<!=SNIP=>
    private Mcp2542 _expander;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initializing...");
        var port = Device.PlatformOS.GetSerialPortName("com1")
            .CreateSerialPort(Mcp2542.DefaultBaudRate);

        _expander = new Mcp2542(port);
        _expander.Start();

        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        while (true)
        {
            await Task.Delay(5000);

            _expander.Send();
        }
    }
    //<!=SNOP=>
}