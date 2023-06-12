public interface IRoomClient
{
    Task ReceivePing(string message);

    Task UpdateRoom(Room room);
}