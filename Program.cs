using System.Threading.Tasks;

using Chat;

Server server = new();

async Task OnClientCreated(int userId)
{
    Console.WriteLine(userId);
    string message = $"Welcome to TCP Chat! Your user id is: {userId}. Send it to anyone so they can text you.";
    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
    await server.SendData(data, userId);
}
server.OnUserAccepted += OnClientCreated;
async Task ServerSide()
{
    await server.HostServer();
}
void OnDataRead(string text)
{
    //string text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    Console.WriteLine(text);
}
async Task ClientSide()
{
    Client client = new();
    client.OnDataRead += OnDataRead;
    client.Connect("127.0.0.1");
    var _ = client.ReadData();
    while (true)
    {
        string userInput = Console.ReadLine();
        var sendData = client.SendData(userInput);

        if (userInput == "q")
        {
            client.CloseConnection();
            break;
        }
    }
}

Console.WriteLine("Program mode(s/c): ");
string choice = Console.ReadLine();

if (choice == "s")
    await ServerSide();
else
    ClientSide();
