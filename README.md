## Table of Content
- [General info](#general-info)
- [Technologies](#technologies)
- [Setup](#setup)
- [Operation](#operation)
## General info
Server for chat operation, working on websockets.
## Technologies
 - C# on .NET 7.0
 - System.Net.Sockets library
 - System.Text.Json library
## Setup
Program needs to be compiled using a compiler that supports .NET 7.0 technologies. 

Client reqires System.Text.Json package, or similar.

toturial: https://learn.microsoft.com/pl-pl/dotnet/fundamentals/networking/sockets/socket-services
## Operation
After connection with a socket is established, send client's username with the Login structure (through JSON formating). Response structure will be sent in turn, informing about the success or failure of connecting. In case of error, send the username once again.

Every time client wants to send a message through Chat, use Message structure. Response structure is sent first to you, with success or error codes. Then Response is sent to other clients connected to Chat.
## JSON structures
### Login
 - username: string
### Message
 - message: string - message sent by user
 - isPrivate: bool - is the message private, or sent to the public chat
 - target: string - if private, to whom the message is sent (username)
### Response
 - code: int
    - 0 - success
    - 1 - invalid formating
    - 2 - invalid username
    - 3 - a new user connected; their username is in "from"
 - error: string - if an error occured, the datailed message is stored here
 - message: string - message sent by another user
 - from: string - from whom the message was sent (username)
 - isPrivate: bool - is the message private, or sent to the public chat
 
 ## Code
 ```cs
if (socket == null)
{
    ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1000);
	socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
	await socket.ConnectAsync(ipEndPoint);
}

var msg = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Login(usernameInput.Text.Trim())));
var res = socket.Send(msg, SocketFlags.None);

var buffer = new byte[1_024];
int received;
received = socket.Receive(buffer, SocketFlags.None);
var response = JsonSerializer.Deserialize<Response>(Encoding.UTF8.GetString(buffer, 0, received));
```