using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

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
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            StringBuilder messageBuilder = new StringBuilder();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            while (stream.DataAvailable);

            string request = messageBuilder.ToString();

            if (request.StartsWith("GET /favicon.ico"))
            {
                client.Close();
                continue;
            }


            Console.WriteLine("Клиент подключился!");

            if (request.StartsWith("GET"))
            {
                string htmlPage = @"
                <html>
                <head>
                    <title>Simple Form</title>
                    <style>
                        body {
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                            font-family: Arial, sans-serif;
                            background-color: #d3d3d3;
                        }
                        .container {
                            text-align: center;
                            border: 1px solid #ccc;
                            padding: 20px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            background-color: #fff;
                        }
                        input[type='text'] {
                            padding: 10px;
                            margin: 10px 0;
                            width: 200px;
                            border: 1px solid #ccc;
                            border-radius: 5px;
                        }
                        input[type='submit'] {
                            padding: 10px 20px;
                            background-color: #28a745;
                            color: white;
                            border: none;
                            border-radius: 5px;
                            cursor: pointer;
                        }
                        input[type='submit']:hover {
                            background-color: #218838;
                        }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Enter your message:</h2>
                        <form method='post'>
                            <input type='text' name='message' />
                            <br />
                            <input type='submit' value='Submit' />
                        </form>
                    </div>
                </body>
                </html>";

                string responseHeader = "HTTP/1.1 200 OK\r\n" +
                                        "Content-Type: text/html\r\n" +
                                        "Content-Length: " + htmlPage.Length + "\r\n\r\n";

                byte[] responseBytes = Encoding.UTF8.GetBytes(responseHeader + htmlPage);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
            else if (request.StartsWith("POST"))
            {
                string[] requestLines = request.Split("\r\n");
                string postData = requestLines[requestLines.Length - 1];
                string message = postData.Split('=')[1];
                message = Uri.UnescapeDataString(message);
                string responseMessage;
                if (ValidateData(message))
                {
                    responseMessage = "Valid data: " + message;
                }
                else
                {
                    responseMessage = "Error: Invalid data.";
                }

                string responseContent = $@"
                <html>
                <head>
                    <title>Result</title>
                    <style>
                        body {{
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                            font-family: Arial, sans-serif;
                            background-color: #d3d3d3;
                        }}
                        .container {{
                            text-align: center;
                            border: 1px solid #ccc;
                            padding: 20px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            background-color: #fff;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>{responseMessage}</h2>
                        <a href='/'>Go back</a>
                    </div>
                </body>
                </html>";

                string responseHeader = "HTTP/1.1 200 OK\r\n" +
                                        "Content-Type: text/html\r\n" +
                                        "Content-Length: " + responseContent.Length + "\r\n\r\n";

                byte[] responseBytes = Encoding.UTF8.GetBytes(responseHeader + responseContent);
                stream.Write(responseBytes, 0, responseBytes.Length);
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
