using System.IO.Ports;
using System.Text;

namespace CardReader
{
    public class SerialDataHandler
    {
        public event EventHandler<SerialDataOpCodeReceivedEventArgs>? SerialDataOpCodeReceived;
        public event EventHandler<SerialDataCardInfoReceivedEventArgs>? SerialDataCardInfoReceived;

        private Dictionary<byte, Func<SerialPort, Task>> opCodes;
        private Dictionary<string, CardInfo> cards;

        private SerialPort port;

        public SerialDataHandler(SerialPort port)
        {
            this.port = port;

            opCodes = new()
            {
                { (byte)SerialDataOpCode.RFID_READ, HandleRfidReadOpCodeAsync },
                { (byte)SerialDataOpCode.RFID_LOCK_STATE, HandleRfidLockStateOpCodeAsync }
            };

            cards = new()
            {
                { "A3 47 64 B7", new CardInfo("Key Tag", new byte[] { 0xA3, 0x47, 0x64, 0xB7 }) },
                { "AC 03 0A E1", new CardInfo("Card Tag", new byte[] { 0xAC, 0x03, 0x0A, 0xE1 }) }
            };
        }

        public void OpenPort()
        {
            if(port.IsOpen)
            {
                return;
            }

            port.Open();
        }

        public void ListenForData()
        {
            if(!port.IsOpen)
            {
                return;
            }

            port.DataReceived += Port_DataReceived;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            if (port is null)
            {
                return;
            }

            if (!port.IsOpen)
            {
                throw new Exception("Serial Port is not open.");
            }

            _ = HandleRecvAsync(port);
        }

        private async Task HandleRecvAsync(SerialPort port)
        {
            byte opCode = (byte)port.ReadByte();

            if (!opCodes.ContainsKey(opCode))
            {
                return;
            }

            SerialDataOpCodeReceived?.Invoke(this, new SerialDataOpCodeReceivedEventArgs() { OpCode = (SerialDataOpCode)opCode });

            await opCodes[opCode](port);
        }

        private async Task HandleRfidReadOpCodeAsync(SerialPort port)
        {
            int len = port.ReadByte();

            byte[] key = new byte[len];

            for (int i = 0; i < len; i++)
            {
                key[i] = (byte)port.ReadByte();
            }

            var sb = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                sb.Append(key[i].ToString("X2"));

                if (i != key.Length - 1)
                {
                    sb.Append(' ');
                }
            }

            string sKey = sb.ToString();

            if (cards.ContainsKey(sKey))
            {
                var cardInfo = cards[sKey];

                SerialDataCardInfoReceived?.Invoke(this, new SerialDataCardInfoReceivedEventArgs() { CardInfo = cardInfo });
            }

            // Wait 2 seconds before unlocking card reader.
            await Task.Delay(2000);

            // Sending any data (more than 0) to the card reader while it's locked will unlock it.
            port.Write(new byte[] { 0x01 }, 0, sizeof(byte));
        }

        private Task HandleRfidLockStateOpCodeAsync(SerialPort port)
        {
            bool lockState = port.ReadByte() > 0 ? true : false;

            return Task.CompletedTask;
        }
    }
}
