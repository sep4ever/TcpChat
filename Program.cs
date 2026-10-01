using System.Threading.Tasks;

using Chat;

async Task ServerSide()
{
    Server server = new();
    await server.HostServer();
}

async Task ClientSide()
{
    Client client = new();
    client.Connect("127.0.0.1");

    while (true)
    {
        string userInput = Console.ReadLine();
        client.SendData(userInput);

        if (userInput == "q")
        {
            client.CloseConnection();
            break;
        }
        var _ = client.ReadData();
    }
}

Console.WriteLine("Program mode(s/c): ");
string choice = Console.ReadLine();

if (choice == "s")
    await ServerSide();
else
    ClientSide();
