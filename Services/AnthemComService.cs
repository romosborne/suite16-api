using System.IO.Ports;
using Microsoft.Extensions.Options;

public interface IAnthemComService
{
    Response SetVolume(double value);
    Response SetInput(string id);
}

public class AnthemComOptions
{
    public const string Position = "AnthemCom";
    public string ComPort { get; set; } = "";
}

public class AnthemComService : IAnthemComService, IDisposable
{
    private readonly object _lock;
    private bool _ready = false;
    private bool _enabled = false;

    private readonly Thread? _bg;

    private readonly List<string> _buffer;
    private readonly SerialPort? _sp;
    private readonly IStateService _state;
    private readonly ILogger<AnthemComService> _logger;
    private readonly Response Ok = new() { Ok = true };

    public AnthemComService(IOptions<AnthemComOptions> options, IStateService state, ILogger<AnthemComService> logger)
    {
        _state = state;
        _logger = logger;
        _buffer = new List<string>();
        _lock = new object();

        if (string.IsNullOrEmpty(options.Value.ComPort))
        {
            _enabled = false;
            return;
        }

        _logger.LogInformation($"Attempting to communicate with the Anthem on port: {options.Value.ComPort}");
        try
        {
            _sp = new SerialPort()
            {
                PortName = options.Value.ComPort,
                BaudRate = 115200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                WriteTimeout = 1000
            };
            _sp.Open();
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to open serial port {options.Value.ComPort}: {e.Message}");
            throw;
        }

        _logger.LogInformation($"Communication open - Attemping state refresh");

        _bg = new Thread(ReadInBackground)
        {
            IsBackground = true
        };
        _bg.Start();
        CompleteRefresh();

        _logger.LogInformation($"Connection Established!  Waiting for state...");
        while (!_ready)
        {
            _logger.LogInformation("Waiting...");
            Thread.Sleep(1000);
        }
        _enabled = true;
        _logger.LogInformation("Ready!");
    }

    private void Send(string a)
    {
        if (!_enabled) return;
        try
        {
            _logger.LogInformation($"Sending: {a}");
            _sp?.Write($"{a}\r");
        }
        catch (TimeoutException)
        {
            _logger.LogError($"Timeout talking to the Anthem");
        }
    }

    private void ReadInBackground()
    {
        while (_sp.IsOpen)
        {
            var command = _sp.ReadLine();
            _logger.LogTrace($"Got: {command}");

            _state.ParseAnthemCommand(command);

            if (command.StartsWith("P1"))
            {
                _ready = true;
                _state.EnableEvents(true);
            }
        }

        _logger.LogWarning("Serial port closed");
    }

    private Response CompleteRefresh()
    {
        Send("P1?");
        return Ok;
    }

    public Response SetVolume(double vol)
    {
        Send($"P1VM{vol:##.##}");
        return Ok;
    }

    public Response SetInput(string id)
    {
        Send($"P1S{id[0]}");
        return Ok;
    }


    public void Dispose()
    {
        _sp?.Close();
    }
}