using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace OS_SocketTCP_Client
{
    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";

        static NetworkStream stream;

        static void Main(string[] args)
        {
            TcpClient client = null;

            try
            {
                client = new TcpClient(address, port);
                stream = client.GetStream();

                new Thread(new ThreadStart(Sending)).Start();

                while (true)
                {
                    var data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] | Ответ сервера: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static public void Sending()
        {
            while (true)
            {
                try
                {
                    int number = new Random().Next(35, 41);
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] | Клиент серверу: {number}");
                    string message = number.ToString();
                    byte[] data = Encoding.Unicode.GetBytes(message);

                    stream.Write(data, 0, data.Length);

                    // (T2) Время между запросами
                    Thread.Sleep(2800);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }
    }
}
