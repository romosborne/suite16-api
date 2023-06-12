using Microsoft.AspNetCore.SignalR;

public class State
{
    private readonly IHubContext<RoomHub, IRoomClient> hub;

    public Room[] Rooms { get; set; }
    public Input[] Inputs { get; set; }

    public Boolean EnableEvents { get; set; } = false;

    public State(IHubContext<RoomHub, IRoomClient> hub)
    {
        Rooms = new Room[]{
            new Room(1, "Mezzanine (1)"),
            new Room(2, "Mezzanine (2)"),
            new Room(3, "Courtyard"),
            new Room(4, "Back Bedroom"),
            new Room(5, "Dog room"),
            new Room(6, "Laura's room"),
            new Room(7, "Office"),
            new Room(8, "8 - Unknown"),
            new Room(9, "Kitchen"),
            new Room(10, "10 - Unknown"),
            new Room(11, "11 - Unknown"),
            new Room(12, "Main Bathroom"),
            new Room(13, "Back Patio"),
            new Room(14, "Dining Room"),
            new Room(15, "15 - Unknown"),
            new Room(16, "Dining Room")
        };

        Inputs = new Input[]{
            new Input(10, "Linn"),
        };
        this.hub = hub;
    }

    public void AdjustRoom(int roomId, Action<Room> f)
    {
        var room = Rooms.Single(r => r.Id == roomId);
        f(room);
        if (EnableEvents) hub.Clients.All.UpdateRoom(room);
    }
}