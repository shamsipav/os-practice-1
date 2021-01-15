using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OS_SocketTCP_Client
{
    // -------------------------------------
    // https://metanit.com/sharp/net/4.4.php
    // -------------------------------------

    class Program
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            try
            {
                client = new TcpClient(host, port);
                stream = client.GetStream();

                Thread SendThread = new Thread(new ThreadStart(SendMessage));
                SendThread.Start();

                ReceiveMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void SendMessage()
        {
            while (true)
            {
                try
                {
                    // (T2) Время между запросами: 2.8 секунды
                    Thread.Sleep(2800);

                    // Отправка: Целочисленного значения (из диапазона 35-40)
                    int number = new Random().Next(35, 41);

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] | Сообщение серверу: {number}");

                    string message = number.ToString();
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
    } 
}
