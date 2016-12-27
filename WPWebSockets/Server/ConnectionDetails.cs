using System;
using System.IO;
using System.Net.Sockets;

namespace WPWebSockets.Server
{
    public class ConnectionDetails : WPWebSockets.Server.IConnectionDetails
    {
        public Stream Stream { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public ConnectionType ConnectionType { get; private set; }
        public string Header { get; private set; }
        public Int64 uuid { get; set; }
        public Int64 connectionID { get; set; }
        // this is the path attribute in the first line of the http header
        public string Path { get; private set; }

        public ConnectionDetails (Stream stream, TcpClient tcpClient, string path, ConnectionType connectionType, string header,Int64 _uuid)
        {
            Stream = stream;
            TcpClient = tcpClient;
            Path = path;
            ConnectionType = connectionType;
            Header = header;
            uuid = _uuid;
        }
    }
}
