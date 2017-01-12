using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WPWebSockets.Common;
using WPWebSockets.Server;
using WPWebSockets.Server.Http;

namespace WPWebSocketsCmd.Server
{
    /// <summary>
    /// this class will start the  different websocket services that it hosts.
    /// </summary>
    internal class ServiceFactory : IServiceFactory
    {
        private readonly IWebSocketLogger _logger;
        private readonly string _webRoot;
        private static List<IService> _clients;

        public static List<IService> GetClients()
        {
            return _clients;
        }

        private string GetWebRoot()
        {
            if (!string.IsNullOrWhiteSpace(_webRoot) && Directory.Exists(_webRoot))
            {
                return _webRoot;
            }

            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", string.Empty);
        }

        public ServiceFactory(string webRoot, IWebSocketLogger logger)
        {
            _logger = logger;
            _webRoot = string.IsNullOrWhiteSpace(webRoot) ? GetWebRoot() : webRoot;
            if (!Directory.Exists(_webRoot))
            {
                _logger.Warning(this.GetType(), "Web root not found: {0}", _webRoot);
            }
            else
            {
                _logger.Information(this.GetType(), "Web root: {0}", _webRoot);
            }
        }

        public IService CreateInstance(ConnectionDetails connectionDetails)
        {
            IService srvc = null;
            switch (connectionDetails.ConnectionType)
            {
                case ConnectionType.WebSocket:
                    // you can support different kinds of web socket connections using a different path
                    if (connectionDetails.Path == "/chat")
                    {
                        srvc =  new ChatWebSocketService(connectionDetails.Stream, connectionDetails.TcpClient, connectionDetails.Header, _logger, connectionDetails.uuid);
                    }
                    if (connectionDetails.Path == "/WPS")
                        srvc = new WPSWebSocketService(connectionDetails.Stream, connectionDetails.TcpClient, connectionDetails.Header, _logger,connectionDetails.uuid);
                    break;
                case ConnectionType.Http:
                    // this path actually refers to the reletive location of some html file or image
                    srvc =  new HttpService(connectionDetails.Stream, connectionDetails.Path, _webRoot, _logger);
                    break;

            }
            if (null != srvc)
            {
                if (null == _clients)
                {
                    _clients = new List<IService>();
                }
                
                _clients.Add(srvc);

                return srvc;
            }
            else
                return new BadRequestService(connectionDetails.Stream, connectionDetails.Header, _logger);
        }
    }
}
