using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        private Socket _clientSocket;
        private byte[] _buffer;

        public Session(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _buffer = new byte[1024];
        }

        // 클라이언트와의 연결을 초기화합니다.
        public async Task ConnectAsync()
        {
            // 데이터 수신을 시작합니다.
            await ReceiveDataAsync();
        }

        // 클라이언트로부터 데이터를 비동기적으로 수신합니다.
        private async Task ReceiveDataAsync()
        {
            while (_clientSocket.Connected)
            {
                try
                {
                    int received = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);

                    if (received == 0) // 연결 종료
                    {
                        Disconnect();
                        return;
                    }

                    // 받은 데이터를 처리합니다. 여기서는 간단하게 콘솔에 출력합니다.
                    OnRecv(new ArraySegment<byte>(_buffer, 0, received));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving data: {e.Message}");
                    Disconnect();
                    return;
                }
            }
        }

        // 클라이언트에게 데이터를 비동기적으로 전송합니다.
        public async Task SendDataAsync(string data)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                int sent = await _clientSocket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                OnSend(sent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending data: {e.Message}");
                Disconnect();
            }
        }

        // 연결을 종료합니다.
        public void Disconnect()
        {
            OnDisconnected(_clientSocket.RemoteEndPoint);
            SessionManager.Instance.RemoveSession(this);

            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();
        }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

    }
}