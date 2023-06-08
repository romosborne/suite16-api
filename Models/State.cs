public class State
{
    public Room[] Rooms { get; set; }
    public Input[] Inputs { get; set; }

    public State()
    {
        Rooms = new Room[]{
            new Room(1, "Mezzanine above bed"),
            new Room(2, "Mezzanine by TV"),
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
    }

    public void AdjustRoom(int room, Action<Room> f)
    {
        f(Rooms.Single(r => r.Id == room));
    }
}