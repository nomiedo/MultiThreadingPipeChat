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
        EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        List<string> messages = new List<string>();
        object lockObject = new object();

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
            NamedPipeServerStream pipeStream = new NamedPipeServerStream("TestPipe", PipeDirection.InOut, 254, PipeTransmissionMode.Message);

            StreamReader reader = new StreamReader(pipeStream);
            StreamWriter writer = new StreamWriter(pipeStream);
            Console.WriteLine("Server waiting for a connection...");
            pipeStream.WaitForConnection();

            Console.Write("A client has connected, send a greeting from the server: ");
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            pipeStream.Write(messageBytes, 0, messageBytes.Length);

            string response = ProcessSingleReceivedMessage(pipeStream);
            Console.WriteLine("The client has responded: {0}", response);
            while (response != "x")
            {
                Console.Write("Send a response from the server: ");
                message = Console.ReadLine();
                messageBytes = Encoding.UTF8.GetBytes(message);
                pipeStream.Write(messageBytes, 0, messageBytes.Length);
                response = ProcessSingleReceivedMessage(pipeStream);
                Console.WriteLine("The client is saying {0}", response);
            }

            Console.WriteLine("The client has ended the conversation.");

        }

        //public void Read(StreamReader reader)
        //{
        //    var line = reader.ReadLine();
        //    messages.Add(line);
        //    Console.WriteLine(line);
        //}

        //public void Write(StreamWriter writer, string line)
        //{
        //    messages.Add(line);
        //    writer.WriteLine(line);
        //    writer.Flush();
        //}

        private static string ProcessSingleReceivedMessage(NamedPipeServerStream namedPipeServer)
        {
            StringBuilder messageBuilder = new StringBuilder();
            string messageChunk = string.Empty;
            byte[] messageBuffer = new byte[5];
            do
            {
                namedPipeServer.Read(messageBuffer, 0, messageBuffer.Length);
                messageChunk = Encoding.UTF8.GetString(messageBuffer);
                messageBuilder.Append(messageChunk);
                messageBuffer = new byte[messageBuffer.Length];
            }
            while (!namedPipeServer.IsMessageComplete);
            return messageBuilder.ToString();
        }
    }
}
