namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Mcp2515
{
    private enum InterruptFlags : byte
    {
        /// <summary>
        /// Rx0 buffer full
        /// </summary>
        RX0IF = 0x01,
        /// <summary>
        /// Rx1 buffer full
        /// </summary>
        RX1IF = 0x02,
        /// <summary>
        /// Tx0 buffer empty
        /// </summary>
        TX0IF = 0x04,
        /// <summary>
        /// Tx1 buffer empty
        /// </summary>
        TX1IF = 0x08,
        /// <summary>
        /// Tx2 buffer empty
        /// </summary>
        TX2IF = 0x10,
        /// <summary>
        /// Error
        /// </summary>
        ERRIF = 0x20,
        /// <summary>
        /// Wake
        /// </summary>
        WAKIF = 0x40,
        /// <summary>
        /// Message Error
        /// </summary>
        MERRF = 0x80
    };

}