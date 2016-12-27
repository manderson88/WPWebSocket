using System;
namespace WPWebSockets.Server
{
    interface IConnectionDetails
    {
        Int64 connectionID { get; set; }
        ConnectionType ConnectionType { get; }
    }
}
