using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;

        public void Init(IPEndPoint endPoint)
        {
            // TCP Socket
            _listenSocket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Bind & Listen
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10);
        }

        public async Task<Socket> AcceptAsync()
        {
            return await _listenSocket.AcceptAsync();
        }

        public Socket Accept()
        {
            return _listenSocket.Accept();
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            bool pending = _listenSocket.AcceptAsync(args);
            if(pending == false)
            {
                OnAcceptCompleted();
            }
        }

        void OnAcceptCompleted()
        {

        }
    }
}
