using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

public interface IStateService {
    void ParseSuite16Command(string command);
    void ParseAnthemCommand(string command);
    void EnableEvents(bool enable);
    State GetState();
}

public class StateService : IStateService {
    private readonly State _state;
    private readonly ILogger<StateService> _logger;

    private readonly Regex _anthemInitial = new Regex("P1S(.)V([0-9.-]*)");

    public StateService(IHubContext<RoomHub, IRoomClient> hub, ILogger<StateService> logger) {
        _logger = logger;
        _state = new State(hub);
    }

    public State GetState() {
        return _state;
    }

    public void EnableEvents(bool enable) {
        _state.EnableEvents = enable;
    }

    public void ParseAnthemCommand(string command) {
        _logger.LogTrace($"Parsing {command}");
        if (!command.StartsWith("P1")) {
            _logger.LogTrace($"Ignoring {command}");
            return;
        }

        var match = _anthemInitial.Match(command);
        if (match.Success) {
            var input = match.Groups[1];
            _state.AdjustAnthem(a => a.Input = input.ToString()[0]);
            var vol = match.Groups[2];
            _state.AdjustAnthem(a => a.Volume = double.Parse(vol.ToString()));
            _logger.LogInformation($"Anthem on input {input} at {vol}db");
            return;
        }

        if (command.StartsWith("P1S")) {
            var input = command[3];
            _state.AdjustAnthem(a => a.Input = input);
            _logger.LogInformation($"Anthem input set to {input}");
        }
        else if (command.StartsWith("P1VM")) {
            if (command[4] != '-') {
                _logger.LogTrace($"Ignoring volume ({command[4]})");
            }
            var volStr = command.Substring(4);
            _logger.LogTrace($"V1: {volStr}");
            var volume = double.Parse(volStr);
            _logger.LogInformation($"Anthem volume to {volume}");
            _state.AdjustAnthem((a) => a.Volume = volume);
        }
    }

    public void ParseSuite16Command(string command) {
        _logger.LogTrace($"Parsing {command}");
        var f1 = command.Substring(3, 2);
        var f2 = command.Substring(5, 2);
        _ = int.TryParse(command.AsSpan(8, 2), out var room);
        _logger.LogTrace($"f1: {f1}, f2:{f2}");
        switch (f1) {
            case "AL":
                if (f2 == "OF") {
                    _state.AdjustAllRooms((r) => r.On = false);
                }
                break;

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
                _logger.LogTrace($"Ignoring command: {command}");
                break;
        }
    }
}