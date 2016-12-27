using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace WPWebSocketsCmd
{
    public partial class WPSClientGUI : Form
    {
         string m_infoMessages { get; set; }
         string m_warningMessages { get; set; }
         string m_errorMessages { get; set; }
         private WPSClientGUILogger logger;
        // private Boolean m_running = true;
        public WPSClientGUI()
        {
            InitializeComponent();
            logger = new WPSClientGUILogger(this);
            this.Show();
        }
       
        public WPWebSockets.Common.IWebSocketLogger GetLogger()
        {
            return logger;
        }
        public void SetInformation(string s)
        {
            txtInfo.Invalidate();
            this.txtInfo.SetPropertyValue(a => a.Text, s);
            //txtInfo.Text = s;
        }
        public void SetWarning(string s)
        {
            txtWarning.Invalidate();
            this.txtWarning.SetPropertyValue(a => a.Text, s);
            //txtWarning.Text = s;
        }
        public void SetError(string s)
        {
            txtError.Invalidate();
            this.txtError.SetPropertyValue(a => a.Text, s);
            //txtError.Text = s;
        }
    }
    static class ControlExtension
    {
        delegate void SetPropertyValueHandler<TResult>(Control source, Expression<Func<Control, TResult>> selector, TResult value);

        public static void SetPropertyValue<TResult>(this Control source, Expression<Func<Control, TResult>> selector, TResult value)
        {
            if (source.InvokeRequired)
            {
                var del = new SetPropertyValueHandler<TResult>(SetPropertyValue);
                source.Invoke(del, new object[] { source, selector, value });
            }
            else
            {
                var propInfo = ((MemberExpression)selector.Body).Member as PropertyInfo;
                propInfo.SetValue(source, value, null);
            }

        }

        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
                @this.BeginInvoke(code);
            else
                code.Invoke();
        }
    }
    class WPSClientGUILogger : WPWebSockets.Common.IWebSocketLogger
    {
        WPSClientGUI m_host;
        public WPSClientGUILogger(WPSClientGUI host)
        {
            m_host = host;
        }
        public void Information(Type type, string format, params object[] args)
        {
            m_host.SetInformation(string.Format(format,args));
        }

        public void Warning(Type type, string format, params object[] args)
        {
            m_host.SetWarning(string.Format(format, args));
        }

        public void Error(Type type, string format, params object[] args)
        {
            m_host.SetError(string.Format(format, args));
        }

        public void Error(Type type, Exception exception)
        {
            m_host.SetError(exception.Message);
        }

       
    }
}
