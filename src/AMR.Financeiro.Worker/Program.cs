using AMR.Financeiro.Worker;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<LancamentoCriadoConsumer>();

var host = builder.Build();
host.Run();
