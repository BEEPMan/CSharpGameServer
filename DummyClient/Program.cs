using System.Net;
using System.Net.Sockets;
using System.Text;

IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
IPAddress iPAddress = iPHostEntry.AddressList[0];

IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 60000);

using Socket client = new(
    localEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

await client.ConnectAsync(localEndPoint);
while (true)
{
    // Send message
    var message = "Hello, world!<|EOM|>";
    var messageBytes = Encoding.UTF8.GetBytes(message);
    _ = await client.SendAsync(messageBytes, 0);
    Console.WriteLine($"Socket client sent message: \"{message}\"");

    // Receive ack
    var buffer = new byte[1024];
    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);
    if (response == "<|ACK|>")
    {
        Console.WriteLine(
            $"Socket client received acknowledgment: \"{response}\"");
        break;
    }
}