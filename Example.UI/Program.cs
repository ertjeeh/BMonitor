using BMonitor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBMonitorSqlServer("Server=127.0.0.1,14330;Database=BMonitor;User Id=sa;Password=*******;TrustServerCertificate=True");
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseBMonitor();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();