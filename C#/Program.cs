using System.IO.Ports;

namespace CardReader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var port = new SerialPort()
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                RtsEnable = true,
            };

            var portNames = SerialPortExtensions.GetArduinoPortName();
            if(portNames.Length < 1)
            {
                Console.WriteLine("Could not find port for Arduino Uno.");
                return;
            }

            port.PortName = portNames[0];

            var dataHandler = new SerialDataHandler(port);

            dataHandler.OpenPort();
            Console.WriteLine($"Port opened on port '{port.PortName}'.");

            dataHandler.ListenForData();
            Console.WriteLine("Listening for NFC tag..");

            dataHandler.SerialDataOpCodeReceived += DataHandler_SerialDataOpCodeReceived;
            dataHandler.SerialDataCardInfoReceived += DataHandler_SerialDataCardInfoReceived;

            await Task.Delay(-1);
        }

        private static void DataHandler_SerialDataCardInfoReceived(object? sender, SerialDataCardInfoReceivedEventArgs e)
        {
            if(e.CardInfo is null)
            {
                return;
            }

            Console.WriteLine($"NFC Tag of type '{e.CardInfo.name}' and UID '{SerialPortExtensions.BytesToString(e.CardInfo.uid)}' scanned.");
        }

        private static void DataHandler_SerialDataOpCodeReceived(object? sender, SerialDataOpCodeReceivedEventArgs e)
        {
            Console.WriteLine($"Received OpCode '{e.OpCode}' from Arduino.");
        }
    }
}