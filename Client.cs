using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Chat;

public class Client
{
    public event Action<string>? OnDataRead;
    private TcpClient client;
    public void Connect(string address)
    {
        client = new TcpClient();
        client.Connect(address, Settings.Port);
    }

    public async Task SendData(string message)
    {
        NetworkStream stream = client.GetStream();

        var data = Encoding.UTF8.GetBytes(message + Settings.EndSymbol);

        await stream.WriteAsync(data, 0, data.Length);
    }

    public async Task ReadData()
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
            buffer.Append(text);

            while (TryParseMessage(buffer.ToString(), out string parsedMessage))
            {
                buffer.Remove(0, parsedMessage.Length + 1);
                OnDataRead?.Invoke(parsedMessage);
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

    public void CloseConnection() => client.Close();
}
