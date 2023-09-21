using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetSection("PostgresConfiguration").GetSection("ConnectionString").Value);
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
  x.AddEntityFrameworkOutbox<AuctionDbContext>(o => { 
    o.QueryDelay = TimeSpan.FromSeconds(10);

    o.UsePostgres();
    o.UseBusOutbox();
  });

  x.UsingRabbitMq((context, cfg) => 
  {
    cfg.ConfigureEndpoints(context);
  });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch(Exception e)
{
    Console.WriteLine(e);
}

app.Run();
