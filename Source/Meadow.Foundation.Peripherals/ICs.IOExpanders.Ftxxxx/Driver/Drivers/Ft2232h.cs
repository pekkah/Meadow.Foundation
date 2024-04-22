﻿using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FT2232 USB IO expander
/// </summary>
public class Ft2232h : FtdiExpander
{
    internal Ft2232h()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        // TODO: depends on part
        var bus = new Ft23xxI2cBus(this, busSpeed);
        bus.Configure();
        return bus;
    }

    /// <inheritdoc/>
    public override ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration)
    {
        throw new NotSupportedException();
    }
}
