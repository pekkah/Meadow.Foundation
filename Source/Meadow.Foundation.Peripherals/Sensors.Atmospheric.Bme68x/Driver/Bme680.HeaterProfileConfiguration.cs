﻿using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bme680
    {
        /// <summary>
        /// The heater profile configuration saved on the device.
        /// </summary>
        public class HeaterProfileConfiguration
        {
            /// <summary>
            /// The chosen heater profile slot, ranging from 0-9
            /// </summary>
            public HeaterProfileType HeaterProfile { get; set; }
            /// <summary>
            /// The heater resistance.
            /// </summary>
            public ushort HeaterResistance { get; set; }
            /// <summary>
            /// The heater duration in the internally used format
            /// </summary>
            public byte HeaterDuration { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="profile">The heater profile</param>
            /// <param name="heaterResistance">The heater resistance (Ohms)</param>
            /// <param name="heaterDuration">The heating duration</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public HeaterProfileConfiguration(HeaterProfileType profile, ushort heaterResistance, byte heaterDuration)
            {
                if (!Enum.IsDefined(typeof(HeaterProfileType), profile))
                    throw new ArgumentOutOfRangeException();

                HeaterProfile = profile;
                HeaterResistance = heaterResistance;
                HeaterDuration = heaterDuration;
            }

            /// <summary>
            /// Gets the configured heater duration in ms
            /// </summary>
            /// <returns></returns>
            public ushort GetHeaterDurationInMilliseconds()
            {
                var factorLookup = new[] { 1, 4, 16, 64 };
                var factor = factorLookup[HeaterDuration >> 6];
                var value = HeaterDuration & 0b0011_1111;

                return (ushort)(factor * value);
            }
        }
    }
}