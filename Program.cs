using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace ChatServer
{
    
    class Program
    {

        static TcpListener server= null;
        static string whois = "Я";

        static void Main(string[] args)
        {

            IPAddress localhost = IPAddress.Loopback;
            server = new TcpListener(localhost, 14333);

            Thread t = new Thread(delegate ()
            {

                server.Start();

                try
                {
                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                        Thread t = new Thread(new ParameterizedThreadStart(Job));
                        t.Start(client);
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                    server.Stop();
                }

            });
            t.Start();

            Console.WriteLine("Server Started...!");
        }

        static void Job(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            string imei = String.Empty;
            string data = null;
            Byte[] bytes = new Byte[256];
            int i;
            string name = "";
            try
            {
                bool hello = true;
                bool questions = false;
                string str="";
                

                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.Unicode.GetString(bytes, 0, i);
                    Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);

                    str = "Что скажешь?";

                    if (questions)
                    {
                        name = ", " + data;
                        whois += name;
                        str = data+", cпроси 'кто здесь?' или 'как дела?', 'пока' - закончить беседу.";
                        questions = false;
                    }

                    if (hello)
                    { 
                        str = "Привет, как тебя зовут?";
                        hello = false;
                        questions = true;
                    }

                    if (data.Equals("как дела?")) { str = "хорошо"; }

                    if (data.Equals("кто здесь?")) { str = whois; }

                    if (data.Equals("пока")) 
                    { 
                        str = "пока";
                        break;
                    }


                    Byte[] reply = System.Text.Encoding.Unicode.GetBytes(str);
                    stream.Write(reply, 0, reply.Length);
                    Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
            whois = whois.Remove(whois.IndexOf(name), name.Length);
        }

    }
}
