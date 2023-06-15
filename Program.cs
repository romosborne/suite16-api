var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
});

// Add services to the container.
builder.Services.Configure<Suite16ComOptions>(builder.Configuration.GetRequiredSection(Suite16ComOptions.Position));
builder.Services.Configure<AnthemComOptions>(builder.Configuration.GetRequiredSection(AnthemComOptions.Position));
builder.Services.AddSingleton<IStateService, StateService>();
builder.Services.AddSingleton<ISuite16ComService, Suite16ComService>();
builder.Services.AddSingleton<IAnthemComService, AnthemComService>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseCors(p =>
{
    p.AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:5173", "http://speakers.lan")
        .AllowCredentials();
});

app.MapHub<RoomHub>("/roomHub");

// var dns = new MdnsService();

app.Run();
