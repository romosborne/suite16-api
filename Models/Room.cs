public class Room {
    public int Id { get; set; }
    public int InputId { get; set; }
    public string Name { get; set; }
    public bool On { get; set; }
    public bool Mute { get; set; }
    public int Volume { get; set; }
    public int MaxVol { get; set; } = 40;
    public int Bass { get; set; }
    public int Treble { get; set; }
    public int Balance { get; set; }
    public bool LoudnessContour { get; set; }
    public bool StereoEnhance { get; set; }
    public Phonic Phonic { get; set; }

    public Room(int id, string name) {
        Id = id;
        Name = name;
    }
}