﻿// See https://aka.ms/new-console-template for more information
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using System.Diagnostics;

Console.WriteLine("HELLO FROM THE WILDERNESS FT232 DRIVER!");

Ft232Collection.Devices.Refresh();
var count = Ft232Collection.Devices.Count;

Console.WriteLine($"{count} expander{(count == 1 ? string.Empty : "s")} found");
if (count > 0)
{
    foreach (var expander in Ft232Collection.Devices)
    {
        Console.WriteLine($"  SN: {expander.SerialNumber}");
    }
}

var ft232h = new Ft232h_old();

//await TestBME280(ft232);
await TestIli9341(ft232h);
//await TestGpio(ft232);

async Task TestBME280(Ft232h_old expander)
{
    var bme = new Bme280(expander.CreateI2cBus());

    while (true)
    {
        var reading = await bme.Read();
        Debug.WriteLine($"Temp: {reading.Temperature.Value.Fahrenheit}F Humidity: {reading.Humidity.Value.Percent}%");
        await Task.Delay(1000);
    }
}

async Task TestGpio(Ft232h_old expander)
{
    var outputs = new List<IDigitalOutputPort>
    {
        expander.CreateDigitalOutputPort(expander.Pins.C0),
        expander.CreateDigitalOutputPort(expander.Pins.C1),
        expander.CreateDigitalOutputPort(expander.Pins.C2),
        expander.CreateDigitalOutputPort(expander.Pins.C3),
        expander.CreateDigitalOutputPort(expander.Pins.C4),
        expander.CreateDigitalOutputPort(expander.Pins.C5),
        expander.CreateDigitalOutputPort(expander.Pins.C6),
        expander.CreateDigitalOutputPort(expander.Pins.D7),
};

    var s = false;

    while (true)
    {

        for (var i = 0; i < outputs.Count; i++)
        {
            var setTo = (i % 2 == 0) ? s : !s;
            outputs[i].State = setTo;
        }

        await Task.Delay(1000);
        s = !s;
    }
}

async Task TestIli9341(Ft232h_old expander)
{
    var ili = new Ili9341(
        expander.CreateSpiBus(),
        expander.CreateDigitalOutputPort(expander.Pins.C0),
        expander.CreateDigitalOutputPort(expander.Pins.C2),
        expander.CreateDigitalOutputPort(expander.Pins.C1),
        480,
        320
        );

    while (true)
    {
        ili.Fill(Color.Red);
        await Task.Delay(1000);
        ili.Fill(Color.Green);
        await Task.Delay(1000);
        ili.Fill(Color.Blue);
        await Task.Delay(1000);
    }
}