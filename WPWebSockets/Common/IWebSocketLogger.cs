using System;

namespace WPWebSockets.Common
{
    public interface IWebSocketLogger
    {
        void Information(Type type, string format, params object[] args);
        void Information(Type type, string msg);
        void Warning(Type type, string format, params object[] args);
        void Error(Type type, string format, params object[] args);
        void Error(Type type, Exception exception);
    }
}
