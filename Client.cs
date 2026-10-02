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

        var data = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(data, 0, data.Length);
    }

    public async Task ReadData()
    {
        NetworkStream stream = client.GetStream();

        var bytes = new byte[256];

        while (true)
        {
            int count = await stream.ReadAsync(bytes);
            if (count == 0)
                break;
            string text = Encoding.UTF8.GetString(bytes, 0, count);

            OnDataRead?.Invoke(text);
        }
    }

    public void CloseConnection() => client.Close();
}
