using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace GameServer
{
    class Program
    {
        public static StreamWriter Log = new StreamWriter($"C:\\Logs/log_Server.txt");

        static Listener _listener = new Listener();

        public static GameRoom Room = new GameRoom();

        public static int TotalRecvCount = 0;
        public static int TotalSendCount = 0;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.AddSession(); });

            while (true)
            {

            }
        }

        static void OnExit(object sender, EventArgs e)
        {
            Log.WriteLine($"Disconnected(Total Send Packet: {TotalSendCount}, Total Receive Packet: {TotalRecvCount})");
            Log.Close();
        }
    }
}