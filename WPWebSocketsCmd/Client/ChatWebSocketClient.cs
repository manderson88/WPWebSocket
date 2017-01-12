using System;
using System.Text;
using WPWebSockets.Client;
using WPWebSockets.Common;

namespace WPWebSocketsCmd.Client
{
    class ChatWebSocketClient : WebSocketClient
    {
        public ChatWebSocketClient(bool noDelay, IWebSocketLogger logger,Int64 uuid) : base(noDelay, logger,uuid)
        {
            
        }
        override public void Send(string text) 
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            Send(WebSocketOpCode.TextFrame, buffer);
        }
       
    }
}
