using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QlikView.Qvx.QvxLibrary;

namespace QvNLogicConnector
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length == 4 && args[0] == "debug")
            {
                Console.WriteLine("Test mode");

                var connection = new QvNLogicConnection();
                connection.MParameters = new Dictionary<string, string> {
                    { "UserId", args[2] },
                    { "Password", args[3] }
                };
                connection.Init();

                var table = connection.ExtractQuery(File.ReadAllText(args[1]), connection.MTables);
                var rows = connection.MTables[0].GetRows();

                var textProperty = typeof(QvxDataValue)
                    .GetProperty("Text", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var row in rows)
                {
                    var values = typeof(QvxDataRow)
                        .GetField("m_Values", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(row)
                        as Dictionary<string, QvxDataValue>;

                    foreach (var kv in values)
                        Console.Write($"{kv.Key}:\t{textProperty.GetValue(kv.Value)}, ");
                    Console.WriteLine();
                }
            }
            else if (args != null && args.Length >= 2)
            {
                new QvNLogicServer().Run(args[0], args[1]);
            }
        }
    }
}
