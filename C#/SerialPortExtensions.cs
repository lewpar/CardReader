using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace CardReader
{
    public static class SerialPortExtensions
    {
        public static string[] GetArduinoPortName()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SerialPort.GetPortNames();
            }

            ManagementScope scope = new ManagementScope();
            SelectQuery query = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            try
            {
                foreach (var item in searcher.Get())
                {

                    var descObject = item["Description"];
                    var devIdObject = item["DeviceID"];

                    if (descObject is null ||
                        devIdObject is null)
                    {
                        continue;
                    }

                    var description = descObject.ToString();
                    var deviceID = devIdObject.ToString();

                    if (description is null ||
                        deviceID is null)
                    {
                        continue;
                    }

                    if (description.Contains("Arduino Uno"))
                    {
                        return new string[] { deviceID };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to automatically find port with error: {ex.Message}");
            }

            return new string[] { };
        }

        public static string BytesToString(this byte[] data)
        {
            if(data.Length < 1)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for(int i = 0; i < data.Length; i++)
            {
                var b = data[i];

                sb.Append(b.ToString("X2"));

                if(i < data.Length - 1)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }
    }
}
