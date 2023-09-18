# Meadow.Foundation.Sensors.Atmospheric.HC2

**HC2 Humidity Probe**

The **HC2** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

``` csharp
HC2 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    // Analog
    //sensor = new HC2(Device.Pins.A00, Device.Pins.A01);

    // Serial
    sensor = new HC2(Device, Device.PlatformOS.GetSerialPortName("COM4"));

    var consumer = HC2.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Temp changed by threshold; new Temp: {result.New.Temperature?.Celsius:N2} °C, old: {result.Old?.Temperature?.Celsius:N2} °C");
        },
        filter: result =>
        {
            // C# 8 pattern match syntax. checks for !null and assigns var.
            if (result.Old is { } old)
            {
                if (result.New.Temperature.HasValue && old.Temperature.HasValue)
                    return ((result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5);
            }
            return false;
        }
    );
    sensor.Subscribe(consumer);

    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"Relative Humidity: {result.New.Humidity?.Percent:N2} %, Temperature: {result.New.Temperature?.Celsius:N2} °C");
    };
    return Task.CompletedTask;
}

public override async Task Run()
{
    Resolver.Log.Info($"Initial Read:");
    var conditions = await sensor.Read();
    Resolver.Log.Info($"Relative Humidity: {conditions.Humidity?.Percent:N2} %, Temperature: {conditions.Temperature?.Celsius:N2} °C");

    sensor.StartUpdating(TimeSpan.FromSeconds(5));
}
```
