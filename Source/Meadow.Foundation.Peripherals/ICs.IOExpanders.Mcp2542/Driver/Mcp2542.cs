using Meadow.Hardware;
using System;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Mcp2542
{
    public const int DefaultBaudRate = 250000;

    private ISerialPort _communicationPort;
    private byte[] _rxBuffer = new byte[256];

    public Mcp2542(ISerialPort communicationPort)
    {
        _communicationPort = communicationPort;
        _communicationPort.DataReceived += OnSerialDataReceived;

        Resolver.Log.Info("MCP2542 created");
    }

    public void Send()
    {
        _communicationPort.Write(Encoding.ASCII.GetBytes("Hello Meadow"));
    }

    public void Start()
    {
        _communicationPort.Open();
        Resolver.Log.Info("Opened");
        _communicationPort.Write(new byte[] { 0x13, 0x10 });

    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        Resolver.Log.Info($"Bytes available={_communicationPort.BytesToRead}");
        var read = _communicationPort.Read(_rxBuffer, 0, _communicationPort.BytesToRead);
        Resolver.Log.Info($"  read {read}: [{BitConverter.ToString(_rxBuffer, 0, read)}]");

    }
}