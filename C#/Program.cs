using System.IO.Ports;

namespace SerialTest
{
    enum ArduinoOpCodes : byte
    {
        RFID_READ = 0x01,
        RFID_LOCK_STATE = 0x02
    }

    internal class Program
    {
        private static Dictionary<byte, Func<SerialPort, Task>> opCodes = new()
        {
            { (byte)ArduinoOpCodes.RFID_READ, HandleRfidReadOpCodeAsync },
            { (byte)ArduinoOpCodes.RFID_LOCK_STATE, HandleRfidLockStateOpCodeAsync }
        };

        static async Task Main(string[] args)
        {
            var ports = SerialPort.GetPortNames();
            int? selectedPort = null;

            while (selectedPort is null)
            {
                Console.WriteLine("Select COM port to continue:");
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine($"{i}: {ports[i]}");
                }

                var option = Console.ReadLine();

                if (!int.TryParse(option, out int result))
                {
                    Console.WriteLine("Invalid option.");
                    Console.WriteLine();
                    continue;
                }

                selectedPort = result;
            }

            Console.WriteLine($"Port '{ports[selectedPort.Value]}' selected.");

            var port = new SerialPort()
            {
                PortName = ports[selectedPort.Value],
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                RtsEnable = true,
            };

            port.Open();

            Console.WriteLine("Waiting for NFC Tag..");

            port.DataReceived += Port_DataReceived;

            await Task.Delay(-1);
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            if (port is null)
            {
                return;
            }

            if (!port.IsOpen)
            {
                Console.WriteLine("Serial Port not open.");
                return;
            }

            Console.WriteLine("Received data from Arduino..");

            _ = HandleRecvAsync(port);
        }

        static async Task HandleRecvAsync(SerialPort port)
        {
            byte opCode = (byte)port.ReadByte();

            Console.WriteLine($"Received OpCode: {opCode}");

            if (!opCodes.ContainsKey(opCode))
            {
                Console.WriteLine($"Received invalid OpCode: {opCode}");
                return;
            }

            await opCodes[opCode](port);
        }

        static async Task HandleRfidReadOpCodeAsync(SerialPort port)
        {
            Console.WriteLine("Handling TestMessage OpCode");

            int len = port.ReadByte();

            Console.WriteLine($"Got length: {len}");

            byte[] key = new byte[len];

            for (int i = 0; i < len; i++)
            {
                key[i] = (byte)port.ReadByte();
            }

            Console.Write("Got NFC Tag UID: ");
            foreach (byte b in key)
            {
                Console.Write($"{b:X} ");
            }

            Console.WriteLine();

            // Wait 2 seconds before unlocking card reader.
            await Task.Delay(2000);

            // Sending any data (more than 0) to the card reader while it's locked will unlock it.
            Console.WriteLine("Sending unlock signal to Arduino..");
            port.Write(new byte[] { 0x01 }, 0, sizeof(byte));
        }

        static Task HandleRfidLockStateOpCodeAsync(SerialPort port)
        {
            Console.WriteLine("Handing LockState OpCode");

            bool lockState = port.ReadByte() > 0 ? true : false;
            Console.WriteLine($"Arduino Reader is: {(lockState ? "Locked" : "Unlocked")}");

            return Task.CompletedTask;
        }
    }
}