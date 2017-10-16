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
            var client = new NamedPipeClientStream(".", "TestPipe", PipeDirection.InOut);
            
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);
            string[] messages = new string[10]
            {
                "Here's a little song I wrote you might want to sing it note for note",
                "Don't worry be happy",
                "In every life we have some trouble but when you worry you make it double",
                "Ain't got no place to lay your head somebody came and took your bed",
                "The landlord say your rent is late he may have to litigate",
                "Look at me I'm happy",
                "Here I'll give you my phone number when you're worried call me I'll make you happy",
                "Ain't got no cash ain't got no style",
                "Ain't got no gal to make you smile",
                "So don't worry be happy"
            };

            Guid name = Guid.NewGuid();
            Random rnd = new Random();

            // connect to server
            client.Connect(3000);

            Console.WriteLine("Client connected to the named pipe server. Waiting for server to send the first message...");
            client.ReadMode = PipeTransmissionMode.Message;
            string messageFromServer = ProcessSingleReceivedMessage(client);
            Console.WriteLine("The server is saying {0}", messageFromServer);
            Console.Write("Write a response: ");
            string response = Console.ReadLine();
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            client.Write(responseBytes, 0, responseBytes.Length);
            while (response != "x")
            {
                messageFromServer = ProcessSingleReceivedMessage(client);
                Console.WriteLine("The server is saying {0}", messageFromServer);
                Console.Write("Write a response: ");
                response = Console.ReadLine();
                responseBytes = Encoding.UTF8.GetBytes(response);
                client.Write(responseBytes, 0, responseBytes.Length);
            }

            //while (true)
            //{
            //    string message = $"{name} : {messages[rnd.Next(9)]}";
            //    writer.WriteLine(message);
            //    writer.Flush();
            //    var line = reader.ReadLine();
            //    Console.WriteLine(line);
            //}



        }

        private static string ProcessSingleReceivedMessage(NamedPipeClientStream namedPipeClient)
        {
            StringBuilder messageBuilder = new StringBuilder();
            string messageChunk = string.Empty;
            byte[] messageBuffer = new byte[5];
            do
            {
                namedPipeClient.Read(messageBuffer, 0, messageBuffer.Length);
                messageChunk = Encoding.UTF8.GetString(messageBuffer);
                messageBuilder.Append(messageChunk);
                messageBuffer = new byte[messageBuffer.Length];
            }
            while (!namedPipeClient.IsMessageComplete);
            return messageBuilder.ToString();
        }
    }
}
