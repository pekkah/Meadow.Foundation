using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public class CanFrame
{
    public uint CanID { get; set; }
    public byte[] Data { get; set; }
}

public enum CanBitRate
{
    BitRate_1MHz = 1000,
    BitRate_500KHz = 500,
    BitRate_250KHz = 250,
    BitRate_200KHz = 200,
    BitRate_125KHz = 125,
    BitRate_100KHz = 100,
    BitRate_80KHz = 80,
    BitRate_50KHz = 50,
    BitRate_40KHz = 40,
}

public partial class Mcp2515 : ISpiPeripheral
{
    private ISpiCommunications _comms;
    private int[] _configParams;

    /// <inheritdoc/>
    public ISpiBus SpiBus { get; }

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode SpiBusMode { get; set; }

    /// <inheritdoc/>
    public Frequency DefaultSpiBusSpeed => new Frequency(5, Frequency.UnitType.Megahertz);

    /// <inheritdoc/>
    public Frequency SpiBusSpeed { get; set; }

    private static int[][] _configTable = new int[][]
    {
        // https://github.com/electricimp/MCP2515?tab=readme-ov-file#timing-suggestions
        // |   0   |  1  |  2  |    3    |     4     |     5     |  6  |
        // | clock | bus | BRP | PropSeg | PhaseSeg1 | PhaseSeg2 | SJW |
        new int[] { 8, 1000, 1,1,1,1,1 },
        new int[] { 8, 500, 1,2,3,2,1 },
        new int[] { 8, 250, 1,6,5,4,3 },

        new int[] { 10, 1000, 1, 1, 1, 2, 1 },
        new int[] { 10, 500, 1, 2, 4, 3, 1 },
        new int[] { 10, 250, 1,7,7,5,2 },
    };

    private const byte LINE_SAMPLING_1x = 0x00;
    private const byte LINE_SAMPLING_3x = 0x40;
    private const byte WAKE_NOISE_FILTER = 0x40;

    public Mcp2515(ISpiBus spiBus, IDigitalOutputPort chipSelect, CanBitRate canBitRate = CanBitRate.BitRate_500KHz, OscillatorFrequency oscillatorFreq = OscillatorFrequency.MHz_10)
    {
        SpiBus = spiBus;

        _configParams = GetConfigurationForSettings(oscillatorFreq, canBitRate) ?? throw new NotSupportedException();

        _comms = new SpiCommunications(spiBus, chipSelect, DefaultSpiBusSpeed);

        Initialize();

        SetBitRate(canBitRate);

        SetMode(Mode.Normal);
    }

    private int[]? GetConfigurationForSettings(OscillatorFrequency oscillatorFreq, CanBitRate canBusSpeed)
    {
        for (var r = 0; r < _configTable.Length; r++)
        {
            if (_configTable[r][0] == (int)oscillatorFreq)
            {
                if (_configTable[r][1] == (int)canBusSpeed)
                {
                    Resolver.Log.Info($"Config: {string.Join(' ', _configTable[r])}");
                    return _configTable[r];
                }
            }
        }

        return null;
    }

    private void ReportRegisters(Registers start, int count)
    {
        Span<byte> registers = stackalloc byte[count];
        ReadRegisters(start, registers);

        for (var i = 0; i < registers.Length; i++)
        {
            var name = (Registers)((byte)start + i);

            Resolver.Log.Info($"{name}: 0x{registers[i]:x2}");
        }

    }

    private CanFrame? ReadFrame(Registers firstRegister)
    {
        // first read the ID, extended ID and frame
        Span<byte> registers = stackalloc byte[6];
        ReadRegisters(firstRegister, registers);

        uint id = (uint)(registers[1] << 3 + registers[2] >> 5);

        if ((registers[2] & (1 << 3)) != 0)
        {
            // extended ID bit is set
            id = (uint)((id << 2) + (registers[2] & 0x03));
            id = (id << 8) + registers[3];
            id = (id << 8) + registers[4];
            id |= 0x80000000;

        }

        byte dlc = (byte)(registers[5] & 0x0f);
        // TODO: make sure the DLC isn't > max data length (8)

        if ((registers[0] & (1 << 3)) != 0) // remote transfer request (RTR) bit
        {
            id |= 0x40000000;
        }

        Span<byte> data = stackalloc byte[dlc];
        ReadRegisters(firstRegister + 6, data);

        return new CanFrame
        {
            CanID = id,
            Data = data.ToArray(),
        };
    }

    public CanFrame? Read()
    {
        var status = ReadStatus();

        Resolver.Log.Info($"status: 0x{(byte)status.interrupt:x2}");

        if ((status.interrupt & InterruptStatus.RXB0) != 0)
        {
            var f = ReadFrame(Registers.RXB0CTRL);
            // clear the receive buffer interrupt bit
            ModifyRegister(Registers.CANINTF, BitMasks.RX0IF, RegisterBits.Clear);
            return f;
        }
        else if ((status.interrupt & InterruptStatus.RXB1) != 0)
        {
            var f = ReadFrame(Registers.RXB1CTRL);
            ModifyRegister(Registers.CANINTF, BitMasks.RX1IF, RegisterBits.Clear);
            return f;
        }
        else
        {
            // no data to read
            return null;
        }
    }

    public void Reset()
    {
        SendInstruction(Instructions.RESET);
    }

    private void Initialize()
    {
        SetMode(Mode.Config);

        var emptyControlBuffer = new byte[14];
        WriteRegister(Registers.TXB0CTRL, emptyControlBuffer);
        WriteRegister(Registers.TXB1CTRL, emptyControlBuffer);
        WriteRegister(Registers.TXB2CTRL, emptyControlBuffer);

        WriteRegister(Registers.RXB0CTRL, 0);
        WriteRegister(Registers.RXB1CTRL, 0);

        WriteRegister(Registers.CANINTE, (byte)(InterruptFlags.RX0IF | InterruptFlags.RX1IF | InterruptFlags.ERRIF | InterruptFlags.MERRF));

        // clear all RX filters - instead of writing each individually, we just copy a block of zeros to all
        var emptyFilterBuffer = new byte[12];
        WriteRegister(Registers.RXF0SIDH, emptyFilterBuffer);
        // skip CANSTAT and CANCTL
        WriteRegister(Registers.RXF3SIDH, emptyFilterBuffer);

        // clear all RX masks - instead of writing each individually, we just copy a block of zeros to all
        WriteRegister(Registers.RXM0SIDH, new byte[8]);

        ModifyRegister(Registers.RXB0CTRL,
            BitMasks.RXBnCTRL_RXM | BitMasks.RXB0CTRL_BUKT | BitMasks.RXB0CTRL_FILHIT,
            RegisterBits.RXBnCTRL_RXM_STDEXT | RegisterBits.RXB0CTRL_BUKT | RegisterBits.RXB0CTRL_FILHIT);

        ModifyRegister(Registers.RXB1CTRL,
            BitMasks.RXBnCTRL_RXM | BitMasks.RXB1CTRL_FILHIT,
            RegisterBits.RXBnCTRL_RXM_STDEXT | RegisterBits.RXB1CTRL_FILHIT);
    }

    private byte GetTransmitErrorCount()
    {
        Span<byte> buffer = stackalloc byte[1];
        ReadRegisters(Registers.TEC, buffer);
        return buffer[0];
    }

    private byte GetReceivedErrorCount()
    {
        Span<byte> buffer = stackalloc byte[1];
        ReadRegisters(Registers.REC, buffer);
        return buffer[0];
    }

    private void SetMode(Mode mode)
    {
        ModifyRegister(Registers.CANCTRL, BitMasks.CANCTRL_REQOP, mode);

        // TODO: read back the register to make sure it applied
    }

    private void SetBitRate(CanBitRate bitrate)
    {
        var cnf = new byte[3];

        // regisers are in reverse order in memory
        cnf[2] = (byte)(((_configParams[6] - 1) << 6) | (_configParams[2] - 1)); // REG CNF3
        cnf[1] = (byte)(0x80 | LINE_SAMPLING_3x | (_configParams[4] - 1) << 3 | _configParams[3] - 1); // 0x80 == BTLMODE bit, must be set per AppNote 739
        cnf[0] = (byte)(WAKE_NOISE_FILTER | _configParams[5] - 1);
        WriteRegister(Registers.CNF3, cnf);
        Resolver.Log.Info($"CFN3-1: {cnf[0]:x2} | {cnf[1]:x2} | {cnf[2]:x2}");

        Span<byte> buffer = stackalloc byte[3];
        ReadRegisters(Registers.CNF3, buffer);
        Resolver.Log.Info($"CFN3-1: {BitConverter.ToString(buffer.ToArray())}");
    }

    private (InterruptStatus interrupt, Mode mode) ReadStatus()
    {
        ReportRegisters(Registers.CNF3, 6);
        ReportRegisters(Registers.CANSTAT, 2);

        Span<byte> txBuffer = stackalloc byte[2];
        txBuffer[0] = (byte)Instructions.READ;
        txBuffer[1] = (byte)Registers.CANSTAT;
        Span<byte> rxBuffer = stackalloc byte[1];
        _comms.Exchange(txBuffer, rxBuffer);

        return ((InterruptStatus)(rxBuffer[0] & (byte)BitMasks.CANSTAT_ICOD), (Mode)(rxBuffer[0] & (byte)BitMasks.CANSTAT_OPMODE));
    }

    private void SendInstruction(Instructions instruction)
    {
        _comms.Write((byte)instruction);
    }

    private void ModifyRegister(Registers register, BitMasks mask, Mode mode)
    {
        Span<byte> buffer = stackalloc byte[4];
        buffer[0] = (byte)Instructions.BITMOD;
        buffer[1] = (byte)register;
        buffer[2] = (byte)mask;
        buffer[3] = (byte)mode;

        _comms.Write(buffer);
    }

    private void ModifyRegister(Registers register, BitMasks mask, RegisterBits data)
    {
        Span<byte> buffer = stackalloc byte[4];
        buffer[0] = (byte)Instructions.BITMOD;
        buffer[1] = (byte)register;
        buffer[2] = (byte)mask;
        buffer[3] = (byte)data;

        Resolver.Log.Info($"Writing {BitConverter.ToString(buffer.ToArray())}");

        _comms.Write(buffer);
    }

    private void WriteRegister(Registers startRegister, byte value)
    {
        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Instructions.WRITE;
        buffer[1] = (byte)startRegister;
        buffer[2] = value;

        Resolver.Log.Info($"Writing {BitConverter.ToString(buffer.ToArray())}");

        _comms.Write(buffer);
    }

    private void WriteRegister(Registers startRegister, byte[] value)
    {
        Span<byte> buffer = stackalloc byte[value.Length + 2];
        buffer[0] = (byte)Instructions.WRITE;
        buffer[1] = (byte)startRegister;
        value.AsSpan().CopyTo(buffer[2..]);

        Resolver.Log.Info($"Writing {BitConverter.ToString(buffer.ToArray())}");

        _comms.Write(buffer);
    }

    private void ReadRegisters(Registers startRegister, Span<byte> rxBuffer)
    {
        Span<byte> txBuffer = stackalloc byte[2];
        txBuffer[0] = (byte)Instructions.READ;
        txBuffer[1] = (byte)startRegister;
        _comms.Exchange(txBuffer, rxBuffer);
    }
}