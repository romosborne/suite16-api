using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Suite16.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase {
    private readonly IStateService _stateService;
    private readonly ISuite16ComService _comService;
    private readonly IAnthemComService _anthemComService;
    private readonly IHubContext<RoomHub, IRoomClient> _hub;
    private readonly ILogger<RoomController> _logger;

    public RoomController(
        IStateService stateService,
        ISuite16ComService comService,
        IAnthemComService anthemComService,
        IHubContext<RoomHub, IRoomClient> hub,
        ILogger<RoomController> logger) {
        _stateService = stateService;
        _comService = comService;
        _anthemComService = anthemComService;
        _hub = hub;
        _logger = logger;
    }

    [HttpGet]
    public State Get() {
        return _stateService.GetState();
    }

    [HttpGet]
    [Route("{id}")]
    public Room Get(int id) {
        var state = _stateService.GetState();
        return state.Rooms.Single(r => r.Id == id);
    }

    [HttpPost]
    [Route("ping/{value}")]
    public async Task<ActionResult> Ping(string value) {
        await _hub.Clients.All.ReceivePing(value);
        return Ok();
    }

    [HttpPost]
    [Route("anthem/vol/{value}")]
    public ActionResult SetAnthemVol(double value) => Wrapping(() => _anthemComService.SetVolume(value))();

    [HttpPost]
    [Route("anthem/input/{value}")]
    public ActionResult SetAnthemInput(string value) => Wrapping(() => _anthemComService.SetInput(value))();


    [HttpPost]
    [Route("{id}/toggleMute")]
    public ActionResult ToggleMute(int id) => Wrapping(() => _comService.ToggleMute(id))();

    [HttpPost]
    [Route("{id}/vol/{value}")]
    public ActionResult SetVol(int id, int value) => Wrapping(() => _comService.SetVolume(id, value))();

    [HttpPost]
    [Route("{id}/treble/{value}")]
    public ActionResult SetTreble(int id, int value) => Wrapping(() => _comService.SetTreble(id, value))();

    [HttpPost]
    [Route("{id}/bass/{value}")]
    public ActionResult SetBass(int id, int value) => Wrapping(() => _comService.SetBass(id, value))();

    [HttpPost]
    [Route("{id}/loudnessContour/{value}")]
    public ActionResult SetLoudnessContour(int id, int value) => Wrapping(() => _comService.SetLoudnessContour(id, value == 1))();

    [HttpPost]
    [Route("{id}/stereoEnhance/{value}")]
    public ActionResult SetStereoEnhance(int id, int value) => Wrapping(() => _comService.SetStereoEnhance(id, value == 1))();

    [HttpPost]
    [Route("{id}/phonic/{value}")]
    public ActionResult SetPhonic(int id, Phonic value) => Wrapping(() => _comService.SetPhonic(id, value))();

    [HttpPost]
    [Route("{id}/input/{value}")]
    public ActionResult SetInput(int id, int value) => Wrapping(() => _comService.SetInput(id, value))();

    [HttpPost]
    [Route("{id}/on")]
    public ActionResult SetOn(int id) => Wrapping(() => {
        var state = _stateService.GetState();
        return _comService.SetOn(id, state.Rooms.Single(r => r.Id == id).InputId);
    })();

    [HttpPost]
    [Route("{id}/off")]
    public ActionResult SetOff(int id) => Wrapping(() => _comService.SetOff(id))();

    private Func<ActionResult> Wrapping(Func<Response> f) {
        return () => {
            var response = f();
            if (response.Ok) return Ok();
            else return UnprocessableEntity(response.Error);
        };
    }
}
