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
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            // TCP Socket
            _listenSocket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Bind & Listen
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10);

            // Register Accept Event
            _onAcceptHandler += onAcceptHandler;
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>
                (OnAcceptCompleted);
            RegisterAccept(args);
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
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                // TODO
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);
        }
    }
}
