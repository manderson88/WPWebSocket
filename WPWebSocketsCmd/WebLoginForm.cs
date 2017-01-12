using System;
using System.Windows.Forms;

namespace WPWebSocketsCmd
{
    public partial class WebLoginForm : Form
    {
        public DialogResult status { get; set; }
        public string userName { get; set; }
        public string pwd { get; set; }

        public WebLoginForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            status = DialogResult.OK;
            this.Hide();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            status = DialogResult.Cancel;
            this.Hide();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            userName = txtUserName.Text;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            pwd = txtPassword.Text;
        }
    }
}
