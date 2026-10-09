# TcpChat

The name speaks for itself. It's a TCP chat application built with .NET and C#.

This is primarily a learning project, but you're welcome to fork it and use the code in your own applications. The core chat logic is not tied to the console interface.

## Building

Clone the repository and build the project:

```bash
git clone https://github.com/sep4ever/TcpChat.git
cd TcpChat
dotnet build
```

## Starting the Server

Run the application:

```bash
dotnet run
```

When prompted with `Program mode(s/c):`, enter `s` to start the server.

## Exposing the Server with Tailscale Funnel

To make the server accessible over the internet using Tailscale Funnel, run:

```bash
sudo tailscale funnel --tls-terminated-tcp=443 tcp://127.0.0.1:8000
```

Here, `443` is the public port used by Tailscale Funnel, while `8000` is the local port the chat server listens on. The local port is configured in `Settings.cs`.

## Connecting the Client

Run the application again. When prompted with `Program mode(s/c):`, enter `c` to start the client.

You will then be prompted to enter an IP address. Leave the field empty to connect to the server running on your local machine.

**Note:** A connection over the local network is separate from the Tailscale Funnel setup described above. Connecting through Tailscale without Funnel is only possible between devices on the same tailnet.
