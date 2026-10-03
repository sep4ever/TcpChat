using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Chat;

public class Server
{
    public event Action<byte[]> OnDataSent;

    ///<doc>
    ///Pass user ID as int in "Invoke". Mainly used for server messages, displaying user ID.
    public event Func<int, Task> OnUserAccepted;
    private TcpListener? tcpListener;
    private List<TcpClient> connectedClients = new();
    private Dictionary<int, TcpClient> clientIDs = new();
    private Dictionary<int, string> userNames = new();

    private int userCount = 0;

    public async Task HostServer()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, Settings.Port);
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

                var _ = ReadData(tcpClient, userCount);
            }
        }
        catch (SocketException exc)
        {
            Console.WriteLine(exc.Message);
        }
    }

    public bool CheckValidId(int id)
    {
        lock (clientIDs)
            return clientIDs.TryGetValue(id, out TcpClient _);
    }

    public bool SetUserName(int clientId, string newName)
    {
        if (!ValidUserName(newName))
            return false;
        lock (userNames)
        {
            if (!userNames.TryGetValue(clientId, out var _))
            {
                userNames.Add(clientId, newName);
                return true;
            }
            else
            {
                userNames.Remove(clientId);
                userNames.Add(clientId, newName);
                return true;
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

    public async Task ReadData(TcpClient client, int senderId)
    {
        NetworkStream stream = client.GetStream();

        var bytes = new byte[256];
        StringBuilder buffer = new();
        while (true)
        {
            int count = await stream.ReadAsync(bytes);
            if (count == 0)
                break;

            string text = Encoding.UTF8.GetString(bytes, 0, count);
            TcpClient? target;

            if (text.StartsWith(Settings.SetNameCommand))
            {
                string newName = text.Substring(Settings.SetNameCommand.Length).Trim();
                SetUserName(senderId, newName);
                continue;
            }

            buffer.Append(text);
            while (TryParseMessage(buffer.ToString(), out string parsedMessage))
            {
                buffer.Remove(0, parsedMessage.Length + 1);
                if (!TryParseId(parsedMessage, out int id, out string message))
                    continue;
                lock (clientIDs)
                    clientIDs.TryGetValue(id, out target);
                if (target == null)
                    continue;
                string userName = "";
                userNames.TryGetValue(senderId, out userName);
                byte[] data = Encoding.UTF8.GetBytes($"{userName}:{message}\n");
                OnDataSent?.Invoke(data);
                await target.GetStream().WriteAsync(data, 0, data.Length);
            }
        }
    }

    private bool TryParseMessage(string message, out string parsedMessage)
    {
        if (message.Contains(Settings.EndSymbol))
        {
            int messageEndIndex = message.IndexOf(Settings.EndSymbol);
            if (messageEndIndex > 0)
            {
                parsedMessage = message.Substring(0, messageEndIndex);
                return true;
            }
        }
        parsedMessage = string.Empty;
        return false;
    }

    private bool TryParseId(string text, out int id, out string message)
    {
        id = 0;
        message = "";
        int idPostfix = text.IndexOf(Settings.IdPostfix);
        if (idPostfix == -1)
            return false;
        string idPart = text.Substring(0, idPostfix);
        message = text.Substring(idPostfix + 1);
        return Int32.TryParse(idPart, out id);
    }

    public async void AcceptClient()
    {
        TcpClient client = await tcpListener!.AcceptTcpClientAsync();
        connectedClients.Add(client);
    }
}
