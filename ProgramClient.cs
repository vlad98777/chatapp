using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 8888); NetworkStream stream = client.GetStream();

            Console.WriteLine("Подключено к чату!");

            // Вход пользователя
            string username = "";
            Console.Write("Введите логин: ");
            username = Console.ReadLine();
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            stream.Write(usernameBytes, 0, usernameBytes.Length);

            // Запуск поток для получения сообщений
            System.Threading.Thread receiveThread = new System.Threading.Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
                }
            });
            receiveThread.Start();

            // Отправка сообщений в чат
            while (true)
            {
                string message = Console.ReadLine();
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
    }
}