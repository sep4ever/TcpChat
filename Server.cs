using System.Net;
using System.Net.Sockets;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
namespace Chat;

public class Server
{
    public event System.Action<byte[]> OnDataSent;
    ///<doc>
    ///Pass user ID as int in "Invoke". Mainly used for server messages, displaying user ID.
    public event System.Func<int, Task> OnUserAccepted;
    private TcpListener? tcpListener;
    private List<TcpClient> connectedClients = new();
    private Dictionary<int, TcpClient> clientIDs = new();
    private Dictionary<string, TcpClient> userNames = new();

    private int userCount = 0;

    public async Task HostServer()
    {
        try
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
                {
                    clientIDs.Add(Interlocked.Increment(ref userCount), tcpClient);
                    OnUserAccepted?.Invoke(userCount);
                }

                var _ = ReadData(tcpClient);
            }
        }
        catch (SocketException exc)
        {
            Console.WriteLine(exc.Message);
        }
    }

    public bool SetUserName(TcpClient client, string newName)
    {
        if (!ValidUserName(newName))
            return false;
        lock (userNames)
        {
            if (!userNames.TryGetValue(newName, out TcpClient _))
            {
                userNames.Add(newName, client);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private bool ValidUserName(string userName)
    {
        if (Settings.ProhibitedSymbols.ContainsAny(userName))
            return false;
        return true;
    }

    public async Task SendData(byte[] data, int userId)
    {
        try
        {
            NetworkStream stream;
            lock (clientIDs)
            {
                if (clientIDs.TryGetValue(userId, out var client))
                    stream = client.GetStream();
                else
                    return;
            }
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (SocketException exc)
        {
            Console.WriteLine(exc.Message);
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
            OnDataSent?.Invoke(data);
            await target.GetStream().WriteAsync(data, 0, data.Length);
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
