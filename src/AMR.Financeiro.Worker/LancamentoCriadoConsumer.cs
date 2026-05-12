using System.Text;
using System.Text.Json;
using AMR.Financeiro.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AMR.Financeiro.Worker;

public class LancamentoCriadoConsumer : BackgroundService
{
    private readonly ILogger<LancamentoCriadoConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Queue = "lancamento.criado";

    public LancamentoCriadoConsumer(ILogger<LancamentoCriadoConsumer> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            Port     = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:User"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel    = _connection.CreateModel();
        _channel.ExchangeDeclare("lancamentos", ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(Queue, "lancamentos", string.Empty);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var json   = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evento = JsonSerializer.Deserialize<LancamentoCriadoEvent>(json);

            _logger.LogInformation(
                "[RabbitMQ] Lancamento criado | Id: {Id} | Descricao: {Desc} | Valor: {Valor:C} | Em: {Data:G}",
                evento?.LancamentoId, evento?.Descricao, evento?.Valor, evento?.DataCriacao);

            _channel.BasicAck(ea.DeliveryTag, false);
            await Task.CompletedTask;
        };

        _channel.BasicConsume(Queue, false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
