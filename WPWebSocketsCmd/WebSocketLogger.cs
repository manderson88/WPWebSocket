﻿using System;
using System.Diagnostics;
using WPWebSockets.Common;

namespace WPWebSocketsCmd
{
    internal class WebSocketLogger : IWebSocketLogger
    {
        public void Information(Type type, string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void Information(Type t, string msg)
        {
            object[] args = new object[1];
            args[0] = msg;
            Information(t,"{0}",args);
        }

        public void Warning(Type type, string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public void Error(Type type, string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        public void Error(Type type, Exception exception)
        {
            Error(type, "{0}", exception);
        }
    }
}
