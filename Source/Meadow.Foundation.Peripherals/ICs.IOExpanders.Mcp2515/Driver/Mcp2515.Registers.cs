namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Mcp2515
{
    internal enum Status : byte
    {
        RX0IF = (1 << 0),
        RX1IF = (1 << 1)
    }

    internal enum Mode : byte
    {
        Normal = 0x00,
        Sleep = 0x20,
        Loopback = 0x40,
        ListenOnly = 0x60,
        Config = 0x80,
        PowerUp = 0xE0
    }

    internal enum BitMasks : byte
    {
        RXB0CTRL_BUKT = 0x04,
        TXB_EXIDE = 0x08,
        DLC = 0x0F,
        RTR = 0x40,
        RXBnCTRL_RXM = 0x60,
        RXB0CTRL_FILHIT = 0x03,
        RXB1CTRL_FILHIT = 0x07,
        CANCTRL_REQOP = 0xE0,
        RX0IF = 1 << 0,
        RX1IF = 1 << 1,
    }

    internal enum RegisterBits
    {
        Clear = 0,
        RXBnCTRL_RXM_STD = 0x20,
        RXBnCTRL_RXM_EXT = 0x40,
        RXBnCTRL_RXM_STDEXT = 0x00,
        RXBnCTRL_RTR = 0x08,
        RXB0CTRL_BUKT = 0x04,
        RXB0CTRL_FILHIT = 0x00,
        RXB1CTRL_FILHIT = 0x01,
    }

    internal enum Registers : byte
    {
        /// <summary>
        /// FILTER 0 STANDARD IDENTIFIER REGISTER HIGH
        /// </summary>
        RXF0SIDH = 0x00,
        /// <summary>
        /// FILTER 0 STANDARD IDENTIFIER REGISTER LOW
        /// </summary>
        RXF0SIDL = 0x01,
        /// <summary>
        /// FILTER 0 EXTENDED IDENTIFIER REGISTER HIGH
        /// </summary>
        RXF0EID8 = 0x02,
        /// <summary>
        /// FILTER 0 EXTENDED IDENTIFIER REGISTER LOW
        /// </summary>
        RXF0EID0 = 0x03,
        RXF1SIDH = 0x04,
        RXF1SIDL = 0x05,
        RXF1EID8 = 0x06,
        RXF1EID0 = 0x07,
        RXF2SIDH = 0x08,
        RXF2SIDL = 0x09,
        RXF2EID8 = 0x0A,
        RXF2EID0 = 0x0B,
        CANSTAT = 0x0E,
        CANCTRL = 0x0F,
        RXF3SIDH = 0x10,
        RXF3SIDL = 0x11,
        RXF3EID8 = 0x12,
        RXF3EID0 = 0x13,
        RXF4SIDH = 0x14,
        RXF4SIDL = 0x15,
        RXF4EID8 = 0x16,
        RXF4EID0 = 0x17,
        RXF5SIDH = 0x18,
        RXF5SIDL = 0x19,
        RXF5EID8 = 0x1A,
        RXF5EID0 = 0x1B,
        TEC = 0x1C,
        REC = 0x1D,
        /// <summary>
        /// MASK 0 STANDARD IDENTIFIER REGISTER HIGH
        /// </summary>
        RXM0SIDH = 0x20,
        /// <summary>
        /// MASK 0 STANDARD IDENTIFIER REGISTER LOW
        /// </summary>
        RXM0SIDL = 0x21,
        RXM0EID8 = 0x22,
        RXM0EID0 = 0x23,
        RXM1SIDH = 0x24,
        RXM1SIDL = 0x25,
        RXM1EID8 = 0x26,
        RXM1EID0 = 0x27,
        CNF3 = 0x28,
        CNF2 = 0x29,
        CNF1 = 0x2A,
        /// <summary>
        /// Interrupt Enable
        /// </summary>
        CANINTE = 0x2B,
        /// <summary>
        /// Interrupt Flags
        /// </summary>
        CANINTF = 0x2C,
        EFLG = 0x2D,
        TXB0CTRL = 0x30,
        TXB0SIDH = 0x31,
        TXB0SIDL = 0x32,
        TXB0EID8 = 0x33,
        TXB0EID0 = 0x34,
        TXB0DLC = 0x35,
        TXB0DATA = 0x36,
        TXB1CTRL = 0x40,
        TXB1SIDH = 0x41,
        TXB1SIDL = 0x42,
        TXB1EID8 = 0x43,
        TXB1EID0 = 0x44,
        TXB1DLC = 0x45,
        TXB1DATA = 0x46,
        TXB2CTRL = 0x50,
        TXB2SIDH = 0x51,
        TXB2SIDL = 0x52,
        TXB2EID8 = 0x53,
        TXB2EID0 = 0x54,
        TXB2DLC = 0x55,
        TXB2DATA = 0x56,
        /// <summary>
        /// RECEIVE BUFFER 0 CONTROL REGISTER 
        /// </summary>
        RXB0CTRL = 0x60,
        RXB0SIDH = 0x61,
        RXB0SIDL = 0x62,
        RXB0EID8 = 0x63,
        RXB0EID0 = 0x64,
        RXB0DLC = 0x65,
        RXB0DATA = 0x66,
        RXB1CTRL = 0x70,
        RXB1SIDH = 0x71,
        RXB1SIDL = 0x72,
        RXB1EID8 = 0x73,
        RXB1EID0 = 0x74,
        RXB1DLC = 0x75,
        RXB1DATA = 0x76
    }

}