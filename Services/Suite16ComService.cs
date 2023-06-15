using System.IO.Ports;
using Microsoft.Extensions.Options;

public interface ISuite16ComService
{
    Response ToggleMute(int id);
    Response SetVolume(int id, int value);
    Response SetTreble(int id, int value);
    Response SetBass(int id, int value);
    Response SetLoudnessContour(int id, bool value);
    Response SetStereoEnhance(int id, bool value);
    Response SetPhonic(int id, Phonic value);
    Response SetInput(int id, int value);
    Response SetOn(int id, int input);
    Response SetOff(int id);
}

public class Suite16ComOptions
{
    public const string Position = "Suite16Com";
    public string ComPort { get; set; } = "/dev/ttyUSB0";
}

public class Suite16ComService : ISuite16ComService, IDisposable
{
    private readonly object _lock;
    private bool _ready = false;

    private readonly Thread _bg;

    private readonly List<string> _buffer;
    private readonly SerialPort _sp;
    private readonly IStateService _state;
    private readonly ILogger<Suite16ComService> _logger;
    private readonly Response Ok = new() { Ok = true };

    public Suite16ComService(IOptions<Suite16ComOptions> options, IStateService state, ILogger<Suite16ComService> logger)
    {
        _state = state;
        _logger = logger;
        _buffer = new List<string>();
        _lock = new object();

        _logger.LogInformation($"Attempting to communicate with the Suite16 on port: {options.Value.ComPort}");
        try
        {
            _sp = new SerialPort()
            {
                PortName = options.Value.ComPort,
                BaudRate = 19200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                WriteTimeout = 1000,
                NewLine = "\r"
            };
            _sp.Open();
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to open serial port {options.Value.ComPort}: {e.Message}");
            throw;
        }

        _logger.LogInformation($"Communication open - Attemping state refresh");

        _bg = new Thread(ReadInBackground);
        _bg.IsBackground = true;
        _bg.Start();
        //CompleteRefresh();
        _ready = true;
        _logger.LogInformation($"Connection Established!  Waiting for state...");
        while (!_ready)
        {
            _logger.LogInformation("Waiting...");
            Thread.Sleep(1000);
        }
        _logger.LogInformation("Ready!");
    }

    private void Send(string a)
    {
        try
        {
            _logger.LogInformation($"Sending: {a}");
            _sp.Write($"{a}\r");
        }
        catch (TimeoutException)
        {
            _logger.LogError($"Timeout talking to the Suite16");
        }
    }

    private void ReadInBackground()
    {
        while (_sp.IsOpen)
        {
            var command = _sp.ReadLine();
            _logger.LogInformation($"Got: {command}");
            _state.ParseSuite16Command(command);
            if (command == "`AXPGC8R16")
            {
                _ready = true;
                _state.EnableEvents(true);
            }
        }

        _logger.LogWarning("Serial port closed");
    }

    private Response CompleteRefresh()
    {
        Send("`GALRMG00\r");
        return Ok;
    }

    public Response ToggleMute(int room)
    {
        Send($"`SMTOGR{room:00}");
        return Ok;
    }

    public Response SetVolume(int room, int vol)
    {
        Send($"`SV{vol:000}R{room:00}");
        return Ok;
    }

    public Response SetTreble(int id, int value)
    {
        Send($"`ST{value:+00;-00;000}R{id:00}");
        return Ok;
    }

    public Response SetBass(int id, int value)
    {
        Send($"`SB{value:+00;-00;000}R{id:00}");
        return Ok;
    }

    public Response SetLoudnessContour(int id, bool value)
    {
        Send($"`SLD{(value ? "ON" : "OF")}R{id:00}");
        return Ok;
    }

    public Response SetStereoEnhance(int id, bool value)
    {
        Send($"`SSE{(value ? "ON" : "OF")}R{id:00}");
        return Ok;
    }

    public Response SetPhonic(int id, Phonic value)
    {
        switch (value)
        {
            case Phonic.Stereo:
                Send($"`SSTROR{id:00}");
                break;
            case Phonic.MonoLeft:
                Send($"`SMINLR{id:00}");
                break;
            case Phonic.MonoRight:
                Send($"`SMINRR{id:00}");
                break;
        }
        return Ok;
    }


    public Response SetInput(int id, int value)
    {
        Send($"`SAD{value:00}R{id:00}");
        return Ok;
    }

    public Response SetOn(int id, int input)
    {
        Send($"`SAD{input:00}R{id:00}");
        return Ok;
    }

    public Response SetOff(int id)
    {
        Send($"`SRMOFR{id:00}");
        return Ok;
    }

    public void Dispose()
    {
        _sp.Close();
    }
}