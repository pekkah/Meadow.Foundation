using Meadow;
using Meadow.Foundation.ICs.IOExpanders;

public class MeadowApp : App<Windows>
{
    private Ft232h _expander = new Ft232h();
    private Mcp2515 _mcp;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        Console.WriteLine("Creating SPI Bus");

        var bus = _expander.CreateSpiBus();
        _mcp = new Mcp2515(bus, _expander.Pins.C0.CreateDigitalOutputPort());

        Console.WriteLine("Creating Display");

        return base.Initialize();
    }

    public override async Task Run()
    {
        while (true)
        {
            var frame = _mcp.Read();
            if (frame != null)
            {
                Resolver.Log.Info($"ID: {frame.CanID:X8} [{BitConverter.ToString(frame.Data)}]");
            }
            Thread.Sleep(1000);
        }
    }
}
