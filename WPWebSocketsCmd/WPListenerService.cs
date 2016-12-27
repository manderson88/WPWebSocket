using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
namespace WPWebSocketsCmd
{
    class WPListenerService:ServiceBase
    {
        readonly Program _app = new Program();

            public WPListenerService()
            {
                Debugger.Launch();
            }
            protected override void OnStart(string[] args)
            {
                
                Thread ServerThread = new Thread(_app.StartServer);
                RequestAdditionalTime(500);
                ServerThread.Start();
            }
            protected override void OnStop()
            {
                Thread ServerThread = new Thread(() => _app.StopServer());
                ServerThread.Start();
            }

            private void InitializeComponent()
            {
            // 
            // WPListenerService
            // 
            this.ServiceName = "WPListenerService";

            }
        
    }
}
