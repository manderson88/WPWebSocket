using System;

namespace WPWebSockets.Events
{
    public class PongEventArgs : EventArgs
    {
        public byte[] Payload { get; private set; }

        public PongEventArgs(byte[] payload)
        {
            Payload = payload;
        }
    }
}
