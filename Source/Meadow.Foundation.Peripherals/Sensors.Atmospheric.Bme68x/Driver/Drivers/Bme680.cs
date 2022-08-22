﻿using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;

using System.Buffers;
using Meadow.Utilities;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents the Bosch BME680 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    public partial class Bme680:
        SamplingSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)>,
        ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor
    {
        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
       
        /// <summary>
        /// Raised when the pressure value changes
        /// </summary>
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };
       
        /// <summary>
        /// Raised when the humidity value changes
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// The temperature oversampling mode
        /// </summary>
        public Oversample TemperatureOversampleMode
        {
            get => configuration.TemperatureOversample;
            set => configuration.TemperatureOversample = value;
        }
       
        /// <summary>
        /// The pressure oversampling mode
        /// </summary>
        public Oversample PressureOversampleMode
        {
            get => configuration.PressureOversample;
            set => configuration.PressureOversample = value;
        }

        /// <summary>
        /// The humidity oversampling mode
        /// </summary>
        public Oversample HumidityOversampleMode
        {
            get => configuration.HumidityOversample;
            set => configuration.HumidityOversample = value;
        }

        public HeaterProfileType HeaterProfile
        {
            get => heaterProfile;
            set
            {


            }
        }
        HeaterProfileType heaterProfile;

        /// <summary>
        /// Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        /// The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        /// selection to be made in the constructor.
        /// </remarks>
        readonly Bme680Comms bme680Comms;

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current pressure
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// The current humidity, in percent relative humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        readonly Memory<byte> readBuffer = new byte[32];
        readonly Memory<byte> writeBuffer = new byte[32];

        readonly Configuration configuration;

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor.</param>
        public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            configuration = new Configuration();

            bme680Comms = new Bme68xI2C(i2cBus, address);
            
			Initialize();
        }

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="device">The Meadow device to create the chip select port</param>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPin">The chip select pin</param>
        public Bme680(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin) :
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin))
        {
        }

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPort">The chip select pin</param>
        /// <param name="configuration">The BMP680 configuration (optional)</param>
        public Bme680(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration? configuration = null)
        {
            bme680Comms = new Bme68xSPI(spiBus, chipSelectPort);

            this.configuration = (configuration == null) ? new Configuration() : configuration;

            byte value = bme680Comms.ReadRegister(Registers.Status.Address);
            bme680Comms.WriteRegister(Registers.Status.Address, value);

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            // Init the temp and pressure registers
            // Clear the registers so they're in a known state.
            var status = (byte)((((byte)configuration.TemperatureOversample << 5) & 0xe0) |
                                    (((byte)configuration.PressureOversample << 2) & 0x1c));

            bme680Comms.WriteRegister(Registers.ControlTemperatureAndPressure.Address, status);

            // Init the humidity registers
            status = (byte)((byte)configuration.HumidityOversample & 0x07);
            bme680Comms.WriteRegister(Registers.ControlHumidity.Address, status);

            SetGasHeater(new Units.Temperature(320, Units.Temperature.UnitType.Celsius), TimeSpan.FromMilliseconds(150));
        }

        void SetGasHeater(Units.Temperature temperature, TimeSpan heaterTime)
        {
            if(heaterTime == TimeSpan.Zero)
            {
                //turn off 
            }
            else
            {
                bme680Comms.WriteRegister(0x59, (byte)heaterTime.TotalMilliseconds);
            }

        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity) {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> ReadSensor()
        {
            configuration.TemperatureOversample = TemperatureOversampleMode;
            configuration.PressureOversample = PressureOversampleMode;
            configuration.HumidityOversample = HumidityOversampleMode;

            return await Task.Run(() =>
            {
                (Units.Temperature Temperature, RelativeHumidity Humidity, Pressure Pressure) conditions;

                // Read the current control register
                var status = bme680Comms.ReadRegister(Registers.ControlTemperatureAndPressure.Address);

                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);

                bme680Comms.WriteRegister(Registers.ControlTemperatureAndPressure.Address, status);
                // Wait for the sample to be taken.
                do
                {
                    status = bme680Comms.ReadRegister(Registers.ControlTemperatureAndPressure.Address);
                } while (BitHelpers.GetBitValue(status, 0x00));

                var sensorData = readBuffer.Span[0..Registers.AllSensors.Length];
                bme680Comms.ReadRegister(Registers.AllSensors.Address, sensorData);

                var rawPressure = GetRawValue(sensorData.Slice(0, 3));
                var rawTemperature = GetRawValue(sensorData.Slice(3, 3));
                var rawHumidity = GetRawValue(sensorData.Slice(6, 2));
                //var rawVoc = GetRawValue(sensorData.Slice(8, 2));

                bme680Comms.ReadRegister(Registers.CompensationData1.Address, readBuffer.Span[0..Registers.CompensationData1.Length]);
                var compensationData1 = readBuffer.Span[0..Registers.CompensationData1.Length].ToArray();

                bme680Comms.ReadRegister(Registers.CompensationData2.Address, readBuffer.Span[0..Registers.CompensationData2.Length]);
                var compensationData2 = readBuffer.Span[0..Registers.CompensationData2.Length].ToArray();

                var compensationData = ArrayPool<byte>.Shared.Rent(64);
                try
                {
                    Array.Copy(compensationData1, 0, compensationData, 0, compensationData1.Length);
                    Array.Copy(compensationData2, 0, compensationData, 25, compensationData2.Length);

                    var temp = RawToTemperature(rawTemperature,
                        new TemperatureCompensation(compensationData));

                    var pressure = RawToPressure(temp, rawPressure,
                        new PressureCompensation(compensationData));
                    var humidity = RawToHumidity(temp, rawHumidity,
                        new HumidityCompensation(compensationData));

                    conditions.Temperature = new Units.Temperature(temp, TU.Celsius);
                    conditions.Pressure = new Pressure(pressure, PU.Pascal);
                    conditions.Humidity = new RelativeHumidity(humidity, HU.Percent);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compensationData, true);
                }

                return conditions;
            });
        }

        static int GetRawValue(Span<byte> data)
        {
            if (data.Length == 3)
            {
                return (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);
            }
            if (data.Length == 2)
            {
                return (data[0] << 8) | data[1];
            }
            return 0;
        }
        
        static double RawToTemperature(int adcTemperature, TemperatureCompensation temperatureCompensation)
        {   //value is in celcius
            var var1 = ((adcTemperature / 16384.0) - (temperatureCompensation.T1 / 1024.0)) * temperatureCompensation.T2;
            var var2 = (adcTemperature / 131072) - (temperatureCompensation.T1 / 8192.0);
            var var3 = var2 * ((adcTemperature / 131072.0) - (temperatureCompensation.T1 / 8192.0));
            var var4 = var3 * temperatureCompensation.T3 * 16.0;
            var tFine = var1 + var4;
            return tFine / 5120.0;
        }

        static double RawToPressure(double temperature, int adcPressure, PressureCompensation pressureCompensation)
        {
            double var1;
            double var2;
            double var3;
            double calc_pres;

            var PC = pressureCompensation;

            var tFine = temperature * 5120;

            var1 = (tFine / 2.0) - 64000.0;
            var2 = var1 * var1 * (PC.P6 / 131072.0);
            var2 += var1 * PC.P5 * 2.0;
            var2 = (var2 / 4.0) + (PC.P4 * 65536.0);
            var1 = ((PC.P3 * var1 * var1 / 16384.0) + (PC.P2 * var1)) / 524288.0;
            var1 = (1.0f + (var1 / 32768.0)) * (PC.P1);
            calc_pres = (1048576.0 - adcPressure);

            /* Avoid exception caused by division by zero */
            if ((int)var1 != 0)
            {
                calc_pres = (calc_pres - (var2 / 4096.0)) * 6250.0 / var1;
                var1 = PC.P9 * calc_pres * calc_pres / 2147483648.0f;
                var2 = calc_pres * ((PC.P8) / 32768.0);
                var3 = calc_pres / 256.0 * (calc_pres / 256.0) * (calc_pres / 256.0) * (PC.P10 / 131072.0);
                calc_pres += (var1 + var2 + var3 + (PC.P7 * 128.0)) / 16.0;
            }
            else
            {
                calc_pres = 0;
            }

            return calc_pres;
        }

        static double RawToHumidity(double ambientTemperature, int adcHumidity, HumidityCompensation humidityCompensation)
        {
            var var1 = adcHumidity - ((humidityCompensation.H1 * 16.0) + ((humidityCompensation.H3 / 2.0) * ambientTemperature));
            var var2 = var1 * (humidityCompensation.H2 / 262144.0 * (1.0 + (humidityCompensation.H4 / 16384.0 * ambientTemperature) + (humidityCompensation.H5 / 1048576.0 * ambientTemperature * ambientTemperature)));
            var var3 = humidityCompensation.H6 / 16384.0;
            var var4 = humidityCompensation.H7 / 2097152.0;
            return var2 + (var3 + var4 * ambientTemperature) * var2 * var2;
        }

        /*
        double CalculateHeaterRegisterCode(Units.Temperature targetTemperature, double ambientTemperature, GasHeaterCompensation heaterCompensation)
        {
            var var1 = heaterCompensation.Gh1 / 16.0 + 49.0;
            var var2 = heaterCompensation.Gh2 / 32768.0 * 0.0005 + 0.00235;
            var var3 = heaterCompensation.Gh3 / 1024.0;
            var var4 = var1 * (1.0 + (var2 * targetTemperature.Celsius));
            var var5 = var4 + var3 * ambientTemperature;

            var resHeatX = (byte)(3.4 * ((var5 * 4.0 / (4.0 + res))))
        }*/
    }
}