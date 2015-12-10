using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QvNLogicConnector
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        public object Password { get; internal set; }
        public object Username { get; internal set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbUsername.Text?.Trim()) ||
                string.IsNullOrEmpty(tbPassword.Text?.Trim()))
            {
                DialogResult = DialogResult.None;
                return;
            }
            else
            {
                Username = tbUsername.Text.Trim();
                Password = tbPassword.Text.Trim();
            }
        }
    }
}
