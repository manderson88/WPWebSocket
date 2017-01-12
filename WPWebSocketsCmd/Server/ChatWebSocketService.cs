using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using WPWebSockets.Common;
using WPWebSockets.Server.WebSocket;

namespace WPWebSocketsCmd.Server
{
    /// <summary>
    /// the class that handles the client messages inbound.
    /// </summary>
    internal class ChatWebSocketService : WebSocketService
    {
        private readonly IWebSocketLogger _logger;

        public ChatWebSocketService(Stream stream, TcpClient tcpClient, string header, IWebSocketLogger logger,Int64 uuid)
            : base(stream, tcpClient, header, true, logger,uuid)
        {
            _logger = logger;
        }
      
        //this is the received message.  It should decode the message
        // and reply.
        protected override void OnTextFrame(string text)
        {
            string response = "ServerABC: " + text;
            base.Send(response);//send back an ack?
            List<WPWebSockets.Server.IService> conns = ServiceFactory.GetClients();
            base.Broadcast(text, conns, this);
            _logger.Information(this.GetType(), "{0}",response);
            //parse the "command" could  be a json?
        }
        public override void Dispose()
        {
            List<WPWebSockets.Server.IService> conns = ServiceFactory.GetClients();
            
            conns.Remove(this);
            
            base.Dispose();
        }
    }
}
