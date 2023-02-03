using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ChatServer;

int port = 1000;
string adress = "127.0.0.1";
Dictionary<string, Socket> sockets = new();


IPEndPoint ipEndPoint = new(IPAddress.Parse(adress), port);
Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(ipEndPoint);
listener.Listen(port);


while (true)
{
    var s = await listener.AcceptAsync();
    Fred(s);
}

async Task Fred(Socket handler)
{
    var buffer = new byte[1_024];
    int received;
    string username = null;

    Console.WriteLine("connection");

    do
    {
        try
        {
            received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var r = JsonSerializer.Deserialize<Login>(Encoding.UTF8.GetString(buffer, 0, received));
            if (r is not null)
                username = r.username;
        }
        finally
        {
            if (username == null)
                Send(handler, new Response(1, "Invalid formating"));
            else if (sockets.ContainsKey(username))
            {
                Send(handler, new Response(2, "Username already taken"));
                username = null;
            }
            else
                Send(handler, new Response(0));
        }
    } while (username == null);

    sockets.Add(username, handler);

    foreach (var (key, other) in sockets) if (other != handler)
        {
            Send(other, new Response(3, null, null, username));
            Send(handler, new Response(3, null, null, key));
        }

    Console.WriteLine("connected user: " + username);



    while (true)
    {
        Message response = null;
        try
        {
            received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            response = JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(buffer, 0, received));
        }
        finally
        {
            if (response == null)
                Send(handler, new Response(1, "Invalid formating"));
            else
            {
                if (response.isPrivate)
                {
                    if (sockets.ContainsKey(response.target))
                    {
                        Send(handler, new Response(0));
                        Send(sockets[response.target], new Response(0, null, response.message, username, true));
                    }
                    else
                        Send(handler, new Response(2, "Cannot find target"));
                }
                else
                {
                    Send(handler, new Response(0));
                    foreach (var other in sockets.Values) if (other != handler)
                            Send(other, new Response(0, null, response.message, username, false));
                }
            }
        }
    }
}

async void Send(Socket s, object message)
{
    try {
        var msg = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(msg);
        _ = await s.SendAsync(messageBytes, SocketFlags.None);
    } catch (SocketException e) { }
    catch (Exception e) { }
}