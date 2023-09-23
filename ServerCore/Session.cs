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
        private RecvBuffer _recvBuffer = new RecvBuffer(1024);

        public Session(Socket clientSocket)
        {
            _clientSocket = clientSocket;
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
                    _recvBuffer.Clear();
                    ArraySegment<byte> segment = _recvBuffer.WriteSegment;
                    int received = await _clientSocket.ReceiveAsync(segment, SocketFlags.None);

                    if(_recvBuffer.OnWrite(received) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if(processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }
                    
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
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

    }
}