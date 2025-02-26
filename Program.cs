using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Сетевое_программирование_ДЗ4
{
    class Program
    {
        private static List clients = new List(); private static List bannedWords = new List { "badword1", "badword2" }; // Примеры запрещенных слов private static List adminLog = new List(); // Лог администраторских действий

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            Console.WriteLine("Сервер запущен...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                ClientHandler clientHandler = new ClientHandler(client, clients, bannedWords, adminLog);
                clients.Add(clientHandler);
                Thread clientThread = new Thread(clientHandler.HandleClient);
                clientThread.Start();
            }
        }
    }

    class ClientHandler
    {
        private TcpClient _client; private List _clients; private List _bannedWords; private List _adminLog; private string _username; private NetworkStream _stream;

        public ClientHandler(TcpClient client, List<ClientHandler> clients, List<string> bannedWords, List<string> adminLog)
        {
            _client = client;
            _clients = clients;
            _bannedWords = bannedWords;
            _adminLog = adminLog;
            _stream = client.GetStream();
        }

        public void HandleClient()
        {
            try
            {
                RegisterUser();
                ReceiveMessages();
            }
            finally
            {
                _client.Close();
            }
        }

        private void RegisterUser()
        {
            // Реализация регистрации пользователя
            // Здесь можно добавить логику для проверки логина/пароля

            Console.Write("Введите логин: ");
            _username = Console.ReadLine();
            Broadcast($"{_username} вошел в чат.");
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            if (_bannedWords.Any(bannedWord => message.Contains(bannedWord)))
            {
                message = "###"; // Замена запрещенного слова
                _adminLog.Add($"{_username} отправил запрещенное сообщение.");
            }
            Broadcast($"{_username}: {message}");
        }

        private void Broadcast(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clients)
            {
                if (client != this)
                {
                    client._stream.Write(msg, 0, msg.Length);
                }
            }
        }
    }
}
