using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NamedPipeClientStream("TestPipe");
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            Guid name = Guid.NewGuid();
            Random rnd = new Random();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    string message = name + "message" + rnd.Next();
                    writer.WriteLine(message);
                    writer.Flush();
                }
            });

            while (true)
            {
                var line = reader.ReadLine();
                Console.WriteLine(line);
            }
        }
    }
}
