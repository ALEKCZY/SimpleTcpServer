using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class SimpleServer
{
    private TcpListener _server;

    public SimpleServer(string ipAddress, int port)
    {
        _server = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start()
    {
        _server.Start();
        Console.WriteLine("Сервер запущен и ожидает подключений...");

        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            Console.WriteLine("Клиент подключился!");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            StringBuilder messageBuilder = new StringBuilder();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            while (messageBuilder[messageBuilder.Length - 1] != '\n');

            string receivedData = messageBuilder.ToString().TrimEnd('\n');

            // Проверка данных
            if (ValidateData(receivedData))
            {
                Console.WriteLine("Данные корректны: " + receivedData);
                byte[] response = Encoding.UTF8.GetBytes("Данные приняты.");
                stream.Write(response, 0, response.Length);
            }
            else
            {
                Console.WriteLine("Некорректные данные: " + receivedData);
                byte[] response = Encoding.UTF8.GetBytes("Ошибка: данные некорректны.");
                stream.Write(response, 0, response.Length);
            }

            client.Close();
        }
    }

    private bool ValidateData(string data)
    {
        if (string.IsNullOrWhiteSpace(data) || data.Length > 100)
        {
            return false;
        }
        return true;
    }

    public static void Main(string[] args)
    {
        SimpleServer server = new SimpleServer("127.0.0.1", 8080);
        server.Start();
    }
}
