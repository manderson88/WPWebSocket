using System;

namespace WPWebSockets.Events
{
    public class JSONFrameEventArgs : EventArgs
    {
        public string JSON { get; set; }
        public JSONFrameEventArgs(string json)
        {
            JSON = json;
        }
    }
}