namespace CardReader
{
    public class SerialDataOpCodeReceivedEventArgs : EventArgs
    {
        public SerialDataOpCode OpCode { get; set; }
    }
}