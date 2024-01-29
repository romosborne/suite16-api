public interface IRoomClient {
    Task ReceivePing(string message);

    Task UpdateRoom(Room room);

    Task UpdateAnthem(Anthem anthem);
}