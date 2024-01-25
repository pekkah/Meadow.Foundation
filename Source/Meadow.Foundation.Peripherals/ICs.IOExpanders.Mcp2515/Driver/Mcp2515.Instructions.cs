namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Mcp2515
{
    internal enum Instructions : byte
    { // data sheet pg 67
        WRITE = 0x02,
        READ = 0x03,
        BITMOD = 0x05,
        LOAD_TX0 = 0x40,
        LOAD_TX1 = 0x42,
        LOAD_TX2 = 0x44,
        RTS_TX0 = 0x81,
        RTS_TX1 = 0x82,
        RTS_TX2 = 0x84,
        RTS_ALL = 0x87,
        READ_RX0 = 0x90,
        READ_RX1 = 0x94,
        READ_STATUS = 0xA0,
        RX_STATUS = 0xB0,
        RESET = 0xC0
    }

}