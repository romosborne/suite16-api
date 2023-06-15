using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Suite16.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IStateService stateService;
    private readonly ISuite16ComService comService;
    private readonly IAnthemComService anthemComService;
    private readonly IHubContext<RoomHub, IRoomClient> hub;
    private readonly ILogger<RoomController> _logger;

    public RoomController(
        IStateService stateService,
        ISuite16ComService comService,
        IAnthemComService anthemComService,
        IHubContext<RoomHub, IRoomClient> hub,
        ILogger<RoomController> logger)
    {
        this.stateService = stateService;
        this.comService = comService;
        this.anthemComService = anthemComService;
        this.hub = hub;
        _logger = logger;
    }

    [HttpGet]
    public State Get()
    {
        return stateService.GetState();
    }

    [HttpGet]
    [Route("{id}")]
    public Room Get(int id)
    {
        var state = stateService.GetState();
        return state.Rooms.Single(r => r.Id == id);
    }

    [HttpPost]
    [Route("ping/{value}")]
    public async Task<ActionResult> Ping(string value)
    {
        await this.hub.Clients.All.ReceivePing(value);
        return Ok();
    }

    [HttpPost]
    [Route("{id}/toggleMute")]
    public ActionResult ToggleMute(int id) => Wrapping(() => comService.ToggleMute(id))();

    [HttpPost]
    [Route("{id}/vol/{value}")]
    public ActionResult SetVol(int id, int value) => Wrapping(() => comService.SetVolume(id, value))();

    [HttpPost]
    [Route("{id}/treble/{value}")]
    public ActionResult SetTreble(int id, int value) => Wrapping(() => comService.SetTreble(id, value))();

    [HttpPost]
    [Route("{id}/bass/{value}")]
    public ActionResult SetBass(int id, int value) => Wrapping(() => comService.SetBass(id, value))();

    [HttpPost]
    [Route("{id}/loudnessContour/{value}")]
    public ActionResult SetLoudnessContour(int id, int value) => Wrapping(() => comService.SetLoudnessContour(id, value == 1))();

    [HttpPost]
    [Route("{id}/stereoEnhance/{value}")]
    public ActionResult SetStereoEnhance(int id, int value) => Wrapping(() => comService.SetStereoEnhance(id, value == 1))();

    [HttpPost]
    [Route("{id}/phonic/{value}")]
    public ActionResult SetPhonic(int id, Phonic value) => Wrapping(() => comService.SetPhonic(id, value))();

    [HttpPost]
    [Route("{id}/input/{value}")]
    public ActionResult SetInput(int id, int value) => Wrapping(() => comService.SetInput(id, value))();

    [HttpPost]
    [Route("{id}/on")]
    public ActionResult SetOn(int id) => Wrapping(() =>
    {
        var state = stateService.GetState();
        return comService.SetOn(id, state.Rooms.Single(r => r.Id == id).InputId);
    })();

    [HttpPost]
    [Route("{id}/off")]
    public ActionResult SetOff(int id) => Wrapping(() => comService.SetOff(id))();

    private Func<ActionResult> Wrapping(Func<Response> f)
    {
        return () =>
        {
            var response = f();
            if (response.Ok) return Ok();
            else return UnprocessableEntity(response.Error);
        };
    }
}
