using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        private Socket _listenSocket;
        private int _port;
        Func<Session> _sessionFactory;

        public Listener(int port, Func<Session> sessionFactory)
        {
            _port = port;
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;
        }

        public async Task StartAsync()
        {
            // 서버의 IP 주소와 포트 번호를 바인딩합니다.
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, _port));

            // 연결 요청을 수신 대기합니다.
            _listenSocket.Listen(10);

            Console.WriteLine($"Server listening on port {_port}");

            while (true)
            {
                try
                {
                    // 클라이언트의 연결 요청을 비동기적으로 수락합니다.
                    Socket clientSocket = await _listenSocket.AcceptAsync();

                    // 새로운 세션을 생성하고 초기화합니다.
                    Session newSession = _sessionFactory.Invoke();
                    _ = newSession.ConnectAsync();
                    newSession.OnConnected(clientSocket.RemoteEndPoint);
                    SessionManager.Instance.AddSession(newSession);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error accepting client: {e.Message}");
                }
            }
        }
    }
}