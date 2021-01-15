using System;
using System.Collections.Concurrent;
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

        public ConcurrentQueue<Task<string>> tasks = new ConcurrentQueue<Task<string>>();

        public static bool isTaskRun = false;

        public void RunTasks()
        {
            if (tasks.Count > 0)
            {
                while (tasks.Count > 0)
                {
                    tasks.TryDequeue(out Task<string> result);

                    result.Start();
                    isTaskRun = true;
                    Console.WriteLine(isTaskRun);
                    result.Wait();
                    isTaskRun = false;
                    Console.WriteLine(isTaskRun);
                }
            }
        }

        public void Process()
        {
            //Task<string> taskDefault = null;

            bool isThreadStart = false;

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

                        if (isThreadStart == false)
                        {
                            Thread myThread = new Thread(new ThreadStart(RunTasks));
                            myThread.Start();

                            isThreadStart = true;
                        }

                        //if (isTaskRun == true)
                        //{
                        //    continue;
                        //}

                        tasks.Enqueue(new Task<string>(() => GetValue(message)));

                        //taskDefault.ContinueWith(Task => {
                        //    var result = Task.Result;
                        //});
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

        public string GetValue(string message)
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

            return message;
        }

        public void SayHello()
        {
            string line = $"СОЕДИНЕНИЕ УСТАНОВЛЕНО. ВЫ КЛИЕНТ #{IDClient}";

            byte[] data = Encoding.Unicode.GetBytes(line);

            Stream.Write(data, 0, data.Length);
        }
    }
}