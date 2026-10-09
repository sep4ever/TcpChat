using Chat;

Server server = new();
server.OnUserAccepted += OnClientCreated;

async Task OnClientCreated(int userId)
{
    string message = $"Welcome to TCP Chat! Your user id is: {userId}. Send it to anyone so they can text you. \n";
    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
    await server.SendData(data, userId);
}

async Task ServerSide() => await server.HostServer();
void OnDataRead(string text) => Console.WriteLine(text);

async Task ClientSide()
{
    Client client = new();
    client.OnDataRead += OnDataRead;

    Console.Write("Enter IP address (empty for local):");
    string ipAddress = Console.ReadLine();

    if (ipAddress.Trim() != "")
        if (ipAddress.Contains("ts.net"))
        {
            await client.Connect(ipAddress, Settings.FunnelPort, true); //ts funneling.
        }
        else
            await client.Connect(ipAddress, Settings.ServerPort); //local connection, but local among tailnet devices.
    else
        await client.Connect("127.0.0.1", Settings.ServerPort); //purely local connection, used on 1 machine.
    var readData = client.ReadData();
    string prefix = "";
    while (true)
    {
        string userInput = Console.ReadLine();
        if (userInput == "q")
        {
            client.CloseConnection();
            break;
        }

        if (userInput == "/setprefix")
        {
            SetPrefix(ref prefix);
            continue;
        }

        var sendData = client.SendData(prefix + userInput);
    }
}

void SetPrefix(ref string prefix)
{
    Console.Write("Enter other user id (empty to skip): ");
    string userId = Console.ReadLine().Trim();
    prefix = Int32.TryParse(userId, out _) ? userId + Settings.IdPostfix : "";
}

Console.Write("Program mode(s/c): ");
string choice = Console.ReadLine();

if (choice == "s")
    await ServerSide();
else
    await ClientSide();
