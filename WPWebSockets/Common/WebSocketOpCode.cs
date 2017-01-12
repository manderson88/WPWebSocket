﻿
namespace WPWebSockets.Common
{
    public enum WebSocketOpCode
    {
        ContinuationFrame = 0,
        TextFrame = 1,
        BinaryFrame = 2,
        JSONFrame = 3,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 10
    }
}
