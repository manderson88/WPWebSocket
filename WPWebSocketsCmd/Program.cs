using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using WPWebSockets.Common;
using WPWebSocketsCmd;

namespace WPWebSocketsCmd
{
    /// <summary>
    /// this is the entry point for the host application. A page
    /// or other client will connect to this.  This application has been 
    /// derivived from the code at: 
    /// https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp 
    /// it is licensed under the CPOL.  It is free to use and modify.
    /// </summary>
    public class Program
    {
        
/*
#if defined (TESTING_CODE)
        private static void TestClient(object state)
        {
            var logger = (IWebSocketLogger) state;
            using (var client = new ChatWebSocketClient(true, logger))
            {
                Uri uri = new Uri("ws://localhost:8880/chat");
                client.TextFrame += Client_TextFrame;
                client.ConnectionOpened += Client_ConnectionOpened;

                // test the open handshake
                client.OpenBlocking(uri);
            }

            Trace.TraceInformation("Client finished, press any key");
             Console.ReadKey();
        }

        private static void Client_ConnectionOpened(object sender, EventArgs e)
        {
            Trace.TraceInformation("Client: Connection Opened");
            var client = (ChatWebSocketClient) sender;

            // test sending a message to the server
            client.Send("Hi");
        }

        private static void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
            Trace.TraceInformation("Client: {0}", e.Text);
            var client = (ChatWebSocketClient) sender;

            // lets test the close handshake
            client.Dispose();
        }
#endif
 */
        public static void StartUI(Object state)
        {
        }
        
        
        public  void StartServer()
        {
            //WPSClientGUI gui = new WPSClientGUI();
            IWebSocketLogger logger = new WebSocketLogger();

            try
            {
                int port = WPWebSocketsCmd.Properties.Settings.Default.Port;
                string webRoot = WPWebSocketsCmd.Properties.Settings.Default.WebRoot;
                if (!Directory.Exists(webRoot))
                {
                    string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                    logger.Warning(typeof(Program), "Webroot folder {0} not found. Using application base directory: {1}", webRoot, baseFolder);
                    webRoot = baseFolder;
                }

                // used to decide what to do with incoming connections
                WPWebSocketsCmd.Server.ServiceFactory serviceFactory = new WPWebSocketsCmd.Server.ServiceFactory(webRoot, logger);

                using (WPWebSockets.WebServer server = new WPWebSockets.WebServer(serviceFactory, logger))
                {
                    server.Listen(port);
                    //Thread clientThread = new Thread(new ParameterizedThreadStart(StartUI));
                    //clientThread.IsBackground = false;
                    //clientThread.Start();
                     Debug.WriteLine("waiting");

                     //List<IDisposable> _connections = server.GetConnections();

                     //foreach (WPWebSockets.Common.WebSocketBase srv in _connections)
                     //    srv.Send("test broadcast");
                    if(Environment.UserInteractive)
                        while (null != Console.ReadKey()) { StopServer(); }

                    
                    //server.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(typeof(Program), ex);
                Console.ReadKey();
            }
        }
        public  void StopServer()
        {
            if (Application.MessageLoop)
                Application.Exit();
            else
                Environment.Exit(1);
        }
        [STAThread]
        private static void Main(string[] args)
        {
            //IWebSocketLogger logger;// = new WebSocketLogger();
            if (args.Length < 1)
            {
                Console.WriteLine("Required Options:");
                Console.WriteLine("DEBUG : start the debugger");
                Console.WriteLine("GUI : Normal starting mode");
            }

            if ((args.Length > 0) && (args[0].Equals("DEBUG")))
                Debugger.Launch();
            if ((args.Length > 0) && (args[0].Equals("GUI")))
            {
                Program p = new Program();
                p.StartServer();
                return;
            }
            
            if ((args.Length>0) && (args[0].Equals("-service")))
            {
                Debugger.Launch();
                ServiceBase[] servicesToRun = new ServiceBase[]{new WPListenerService()};
                ServiceBase.Run(servicesToRun);
                return;
            } 
           //moved to form?
        }
    }
}
