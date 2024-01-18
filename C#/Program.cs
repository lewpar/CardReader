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

            dataHandler.RegisterRFIDTag(RFIDTag.Key, new byte[] { 0xA3, 0x47, 0x64, 0xB7 });
            dataHandler.RegisterRFIDTag(RFIDTag.Card, new byte[] { 0xAC, 0x03, 0x0A, 0xE1 });

            dataHandler.OpenPort();
            Console.WriteLine($"Port opened on port '{port.PortName}'.");

            dataHandler.ListenForData();
            Console.WriteLine("Listening for NFC tag..");

            dataHandler.SerialDataOpCodeReceived += DataHandler_SerialDataOpCodeReceived;

            dataHandler.SerialDataValidCardInfoReceived += DataHandler_SerialDataValidCardInfoReceived;
            dataHandler.SerialDataInvalidCardInfoReceived += DataHandler_SerialDataInvalidCardInfoReceived;

            await Task.Delay(-1);
        }

        private static void DataHandler_SerialDataInvalidCardInfoReceived(object? sender, SerialDataCardInfoReceivedEventArgs e)
        {
            if(e.CardInfo is null)
            {
                return;
            }

            Console.WriteLine($"[Invalid] NFC Tag of type '{e.CardInfo.name}' and UID '{SerialPortExtensions.BytesToString(e.CardInfo.uid)}' scanned.");
        }

        private static void DataHandler_SerialDataValidCardInfoReceived(object? sender, SerialDataCardInfoReceivedEventArgs e)
        {
            if(e.CardInfo is null)
            {
                return;
            }

            Console.WriteLine($"[Valid] NFC Tag of type '{e.CardInfo.name}' and UID '{SerialPortExtensions.BytesToString(e.CardInfo.uid)}' scanned.");
        }

        private static void DataHandler_SerialDataOpCodeReceived(object? sender, SerialDataOpCodeReceivedEventArgs e)
        {
            Console.WriteLine($"Received OpCode '{e.OpCode}' from Arduino.");
        }
    }
}