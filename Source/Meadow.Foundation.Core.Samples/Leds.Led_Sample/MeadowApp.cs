﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leds.Led_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        List<Led> leds;

        public override Task Initialize(string[]? args)
        {
            var onRgbLed = new RgbLed(
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLedColors.Red);

            leds = new List<Led>
            {
                new Led(Device.Pins.D00),
                new Led(Device.Pins.D01),
                new Led(Device.Pins.D02),
                new Led(Device.Pins.D03),
                new Led(Device.Pins.D04),
                new Led(Device.Pins.D05),
                new Led(Device.Pins.D06),
                new Led(Device.Pins.D07),
                new Led(Device.Pins.D08),
                new Led(Device.Pins.D09),
                new Led(Device.Pins.D10),
                new Led(Device.Pins.D11),
                new Led(Device.Pins.D12),
                new Led(Device.Pins.D13),
                new Led(Device.Pins.D14),
                new Led(Device.Pins.D15)
            };

            onRgbLed.SetColor(RgbLedColors.Green);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("TestLeds...");

            while (true)
            {
                Resolver.Log.Info("Turning on each led every 100ms");
                foreach (var led in leds)
                {
                    led.IsOn = true;
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Turning off each led every 100ms");
                foreach (var led in leds)
                {
                    led.IsOn = false;
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking the LEDs for a second each");
                foreach (var led in leds)
                {
                    led.StartBlink();
                    await Task.Delay(3000);
                    led.Stop();
                }

                Resolver.Log.Info("Blinking the LEDs for a second each with on (1s) and off (1s)");
                foreach (var led in leds)
                {
                    led.StartBlink(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    await Task.Delay(3000);
                    led.Stop();
                }

                await Task.Delay(3000);
            }
        }

        //<!=SNOP=>
    }
}