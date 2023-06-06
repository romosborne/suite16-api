public class State {
    public Room[] Rooms { get; set; }
    public Input[] Inputs { get; set; }

    public State() {
        Rooms = new Room[]{
            new Room(1, "Kitchen"),
            new Room(2, "Butterfly Room"),
            new Room(3, "Courtyard"),
            new Room(4, "Back Patio"),
            new Room(5, "Mezzanine"),
            new Room(6, "Annex"),
            new Room(7, "Garage"),
            new Room(8, "Back Bedroom"),
            new Room(9, "Dog Room"),
            new Room(10, "Laura's Bedroom"),
            new Room(11, "Kitchen"),
            new Room(12, "Kitchen"),
            new Room(13, "Kitchen"),
            new Room(14, "Kitchen"),
            new Room(15, "Kitchen"),
            new Room(16, "Kitchen")
        };

        Inputs = new Input[]{
            new Input(3, "Linn"),
            new Input(5, "Sky"),
            new Input(8, "Sky-q"),
        };
    }

    public void AdjustRoom(int room, Action<Room> f) {
        f(Rooms.Single(r => r.Id == room));
    }
}