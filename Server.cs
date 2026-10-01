using System.Net;
using System.Net.Sockets;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
namespace Chat;

public class Server
{
    private TcpListener? tcpListener;
    private List<TcpClient> connectedClients = new();
    private Dictionary<int, TcpClient> clientIDs = new();
    public async Task HostServer()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        tcpListener = new TcpListener(ipAddress, 8000);

        tcpListener.Start();

        while (true)
        {
            TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
            lock (connectedClients)
                connectedClients.Add(tcpClient);
            lock (clientIDs)
                clientIDs.Add(connectedClients.Count, tcpClient);

            var _ = ReadData(tcpClient);
        }
    }

    public async Task ReadData(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        NetworkStream targetStream;
        var bytes = new byte[256];

        while (true)
        {
            int count = await stream.ReadAsync(bytes);
            if (count == 0)
                break;
            string text = Encoding.UTF8.GetString(bytes, 0, count);
            if (!TryParseId(text, out int id, out string message))
                continue;
            TcpClient? target;
            lock (clientIDs)
                clientIDs.TryGetValue(id, out target);
            if (target == null)
                continue;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await target.GetStream().WriteAsync(data);
            //Console.WriteLine($"Client {clientIDs.FirstOrDefault(c => c.Value == client).Key} sent: " + text);
        }
    }

    private bool TryParseId(string text, out int id, out string message)
    {
        id = 0;
        message = "";
        int colon = text.IndexOf(':');
        if (colon == -1)
            return false;
        string idPart = text.Substring(0, colon);
        message = text.Substring(colon + 1);
        return Int32.TryParse(idPart, out id);
    }

    public async void AcceptClient()
    {
        TcpClient client = await tcpListener!.AcceptTcpClientAsync();
        connectedClients.Add(client);
    }
}
