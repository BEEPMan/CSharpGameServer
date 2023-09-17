using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Listener listener = new Listener(7777); // 포트 3000에서 수신 대기
            await listener.StartAsync();
        }
    }
}