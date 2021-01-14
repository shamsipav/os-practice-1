using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OS_SocketTCP_Server
{
    // -------------------------------------
    // https://metanit.com/sharp/net/4.4.php
    // -------------------------------------

    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }

        // Для идентификации клиента с помощью инкремента
        // ----------------------------------------------
        int IDClient;

        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject, int clientID)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            IDClient = clientID;
        }

        public List<Task> tasks = new List<Task>();

        public void Process()
        {
            try
            {
                Stream = client.GetStream();

                Console.WriteLine($"Клиент {IDClient} подключился к серверу");

                SayHello();

                string message;

                while (true)
                {
                    try
                    {
                        message = GetMessage();

                        Console.WriteLine($"[{ DateTime.Now.ToLongTimeString()}] | Клиент {IDClient}: {message}");

                        // Возникли трудности с реализацией последовательного выполнения задач, согласно варианту (ожидание).
                        // ---------------------------------------------------------------------------------------------------
                        // tasks.Add(Task.Factory.StartNew(() => Test(message)).ContinueWith(Task => tasks[tasks.Count - 2]));
                        // tasks[tasks.Count - 1].Wait();

                        // Следующая строка добавляет задачи в список и сразу же выполняет их параллельно (в течение 6 секунд после добавления)
                        // ------------------------------------------------------------------------------
                        tasks.Add(Task.Factory.StartNew(() => Test(message)));

                    }
                    catch
                    {
                        Console.WriteLine($"[{ DateTime.Now.ToLongTimeString()}] | Клиент {IDClient} прервал соединение");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

        public void Test(string message)
        {
            // (T1) Время обработки запроса: 6 секунд
            Thread.Sleep(6000);

            try
            {
                // Обработка: Вставка значения в заданную исполнителем строку
                string line = $"[{ DateTime.Now.ToLongTimeString()}] | СЕРВЕР: получено значение {message}";

                byte[] data = Encoding.Unicode.GetBytes(line);

                Stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SayHello()
        {
            string line = $"СОЕДИНЕНИЕ УСТАНОВЛЕНО. ВЫ КЛИЕНТ #{IDClient}";

            byte[] data = Encoding.Unicode.GetBytes(line);

            Stream.Write(data, 0, data.Length);
        }
    }
}