using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();

                var connection = new QvNLogicConnection();
                connection.MParameters = new Dictionary<string, string> {
                    { "UserId", args[2] },
                    { "Password", args[3] }
                };
                connection.Init();

                var table = connection.ExtractQuery(File.ReadAllText(args[1]), connection.MTables);
                var rows = connection.MTables[0].GetRows();

                foreach (var row in rows)
                    Console.WriteLine(row);
            }
            else if (args != null && args.Length >= 2)
            {
                new QvNLogicServer().Run(args[0], args[1]);
            }
        }
    }
}
