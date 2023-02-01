namespace ChatServer
{
    public record Login(string username);
    public record Message(string message, string target, bool isPrivate);
    public record Response(int code, string error = null, string message = null, string from = null, bool isPrivate = false);
}