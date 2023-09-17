using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class Program
    {
        private static readonly int PORT = 7777;
        private static readonly string SERVER_IP = "127.0.0.1";
        private static Socket _clientSocket;
        private static byte[] _buffer;

        static async Task Main(string[] args)
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _buffer = new byte[1024];

            try
            {
                await _clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(SERVER_IP), PORT));
                Console.WriteLine("Connected to server.");

                // Send initial message
                await SendDataAsync("Hello, Server!");

                // Start receiving data
                await ReceiveDataAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        static async Task SendDataAsync(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            await _clientSocket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
        }

        static async Task ReceiveDataAsync()
        {
            while (_clientSocket.Connected)
            {
                int received = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);

                if (received == 0)
                {
                    Console.WriteLine("Server disconnected.");
                    return;
                }

                string data = Encoding.UTF8.GetString(_buffer, 0, received);
                Console.WriteLine($"Received from server: {data}");
            }
        }
    }
}