public interface IStateService {
    void ParseCommand(string command);
    State GetState();
}

public class StateService : IStateService {
    private readonly State _state;
    private readonly ILogger<StateService> _logger;

    public StateService(ILogger<StateService> logger) {
        _logger = logger;
        _state = new State();
    }

    public State GetState() {
        return _state;
    }

    public void ParseCommand(string command) {
        _logger.LogTrace($"Parsing {command}");
        var f1 = command.Substring(3, 2);
        var f2 = command.Substring(5, 2);
        _ = int.TryParse(command.AsSpan(8, 2), out var room);
        _logger.LogTrace($"f1: {f1}, f2:{f2}");
        switch (f1) {
            // Room off
            case "RM":
                _state.AdjustRoom(room, (r) => r.On = false);
                break;
            // Input
            case "AD":
                _state.AdjustRoom(room, (r) => {
                    r.InputId = int.Parse(f2);
                    r.On = true;
                    });
                break;
            // Mute
            case "MT":
                switch (f2) {
                    case "ON":
                        _state.AdjustRoom(room, (r) => r.Mute = true);
                        break;
                    case "OF":
                        _state.AdjustRoom(room, (r) => r.Mute = false);
                        break;
                }
                break;
            // Volume
            case "V0":
                _state.AdjustRoom(room, (r) => r.Volume = int.Parse(f2));
                break;
            case "B-":
            case "B0":
            case "B+":
                var bass = int.Parse(command.Substring(4, 3));
                _state.AdjustRoom(room, (r) => r.Bass = bass);
                break;
            case "T-":
            case "T0":
            case "T+":
                var treble = int.Parse(command.Substring(4, 3));
                _state.AdjustRoom(room, (r) => r.Treble = treble);
                break;
            case "LD":
                switch (f2) {
                    case "ON":
                        _state.AdjustRoom(room, (r) => r.LoudnessContour = true);
                        break;
                    case "OF":
                        _state.AdjustRoom(room, (r) => r.LoudnessContour = false);
                        break;
                }
                break;
            case "SE":
                switch (f2) {
                    case "ON":
                        _state.AdjustRoom(room, (r) => r.StereoEnhance = true);
                        break;
                    case "OF":
                        _state.AdjustRoom(room, (r) => r.StereoEnhance = false);
                        break;
                }
                break;
            case "ST":
                _state.AdjustRoom(room, (r) => r.Phonic = Phonic.Stereo);
                break;
            case "MI":
                switch (f2) {
                    case "NL":
                        _state.AdjustRoom(room, (r) => r.Phonic = Phonic.MonoLeft);
                        break;
                    case "NR":
                        _state.AdjustRoom(room, (r) => r.Phonic = Phonic.MonoRight);
                        break;
                }
                break;
            default: 
                _logger.LogWarning($"Ignoring command: {command}");
                break;
        }
    }
}