using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QlikView.Qvx.QvxLibrary;

namespace QvNLogicConnector
{
    internal class QvNLogicServer : QvxServer
    {
        public override QvxConnection CreateConnection()
        {
            return new QvNLogicConnection();
        }

        public override string CreateConnectionString()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateConnectionString()");

            using (var loginDialog = new LoginDialog())
            {
                if (loginDialog.ShowDialog(NativeWindow.FromHandle(MParentWindow)) == DialogResult.OK)
                {
                    return $"UserId={loginDialog.Username};Password={loginDialog.Password}";
                }
            }

            return "";
        }
    }
}
