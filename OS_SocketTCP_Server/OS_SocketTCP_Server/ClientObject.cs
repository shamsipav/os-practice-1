using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OS_SocketTCP_Server
{
    public class ClientObject
    {
        public Dictionary<string, Task<byte[]>> tasks;

        public TcpClient client;
        public int id;

        public ClientObject(TcpClient _client, Dictionary<string, Task<byte[]>> _tasks, int _id)
        {
            client = _client;
            tasks = _tasks;
            id = _id;

            Console.WriteLine($"Клиент {id}");
        }

        public void Process()
        {
            Task<byte[]> taskDefault = null;
            NetworkStream stream = null;

            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64];

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] | Клиент {id}: {message}");

                    if (tasks.ContainsKey(message))
                    {
                        taskDefault = tasks[message];
                    }
                    else
                    {
                        if (taskDefault != null)
                        {
                            if (!taskDefault.IsCompleted)
                            {
                                Sending(Encoding.Unicode.GetBytes("Сервер занят"), stream);
                                continue;
                            }
                        }
                        tasks.Add(message, Task.Factory.StartNew(() => Processing(message)));
                        taskDefault = tasks[message];
                    }

                    taskDefault.ContinueWith(Task => {
                        var result = taskDefault.Result;
                        Sending(result, stream);
                    });
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

        public byte[] Processing(string message)
        {
            // Вставка значения в заданную исполнителем строку
            string line = $"Получено число: {message}";

            // (Т1) Время обработки
            Thread.Sleep(6000);

            tasks.Remove(message);

            return Encoding.Unicode.GetBytes(line);
        }

        public void Sending(byte[] data, NetworkStream Stream)
        {
            Stream.Write(data, 0, data.Length);
        }
    }
}
