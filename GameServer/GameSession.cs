using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace GameServer
{
    class GameSession : Session
    {
        public int SessionId { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("New session started.");

            SessionManager.Instance.BroadCast($"New client entered the server.");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("Session terminated.");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string data = System.Text.Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"Received data: {data}");

            // SendDataAsync(data);
            SessionManager.Instance.BroadCast($"[Client {SessionId}] {data}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            
        }
    }
}
