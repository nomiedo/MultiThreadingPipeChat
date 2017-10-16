using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        bool running;
        Thread runningThread;
        Thread messageThread;
        EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        List<string> messages = new List<string>();
 
        static void Main(string[] args)
        {
            Program server = new Program();
            server.Run();
        }

        void ServerLoop()
        {
            while (running)
            {
                ProcessNextClient();
            }

            terminateHandle.Set();
        }

        public void Run()
        {
            running = true;

            runningThread = new Thread(ServerLoop);
            runningThread.Start();

            Console.WriteLine("Server is started");
        }

        public void Stop()
        {
            running = false;
            terminateHandle.WaitOne();
        }

        public void ProcessNextClient()
        {
            NamedPipeServerStream pipeStream = new NamedPipeServerStream("TestPipe", PipeDirection.InOut, 254);
            StreamReader reader = new StreamReader(pipeStream);
            StreamWriter writer = new StreamWriter(pipeStream);

            pipeStream.WaitForConnection();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    messages.Add(line);
                    Console.WriteLine(line);

                        writer.WriteLine(line);
                        writer.Flush();

                    //foreach (var message in messages)
                    //{
                    //    writer.WriteLine(message);
                    //    writer.Flush();
                    //}
                }
            });

            
        }
    }
}
