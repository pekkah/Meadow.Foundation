# Meadow.Foundation.Sensors.Temperature.Tmp102

**TMP102 I2C temperature sensor**

The **Tmp102** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Tmp102 tmp102;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    tmp102 = new Tmp102(Device.CreateI2cBus());

    var consumer = Tmp102.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Temperature New Value { result.New.Celsius}C");
            Resolver.Log.Info($"Temperature Old Value { result.Old?.Celsius}C");
        },
        filter: null
    );
    tmp102.Subscribe(consumer);

    tmp102.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
    {
        Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:N2}C");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var temp = await tmp102.Read();
    Resolver.Log.Info($"Current temperature: {temp.Celsius} C");

    tmp102.StartUpdating(TimeSpan.FromSeconds(1));
}

```