using System.Net;
using System.Net.Sockets;
using System.Text;

IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
IPAddress iPAddress = iPHostEntry.AddressList[0];

IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 60000);

using Socket listener = new(
    iPAddress.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

listener.Bind(localEndPoint);
listener.Listen(100);

var handler = await listener.AcceptAsync();
while (true)
{
    // Receive message
    var buffer = new byte[1024];
    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);

    var eom = "<|EOM|>";
    if (response.IndexOf(eom) > -1)
    {
        Console.WriteLine(
            $"Socket server received message: \"{response.Replace(eom, "")}\"");

        var ackMessage = "<|ACK|>";
        var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
        await handler.SendAsync(echoBytes, 0);
        Console.WriteLine(
            $"Socket server sent acknowledgment: \"{ackMessage}\"");

        break;
    }
}