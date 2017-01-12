namespace WPWebSocketsCmd
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.WPLServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.WPLServiceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // WPLServiceProcessInstaller1
            // 
            this.WPLServiceProcessInstaller1.Password = null;
            this.WPLServiceProcessInstaller1.Username = null;
            // 
            // WPLServiceInstaller1
            // 
            this.WPLServiceInstaller1.ServiceName = "WPListenerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.WPLServiceProcessInstaller1,
            this.WPLServiceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller WPLServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller WPLServiceInstaller1;
    }
}