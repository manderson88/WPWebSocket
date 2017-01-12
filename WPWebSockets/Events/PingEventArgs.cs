using System;

namespace WPWebSockets.Events
{
    public class PingEventArgs : EventArgs
    {
        public byte[] Payload { get; private set; }

        public PingEventArgs(byte[] payload)
        {
            Payload = payload;
        }
    }
}
