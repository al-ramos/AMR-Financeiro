using System.Text;
using System.Text.Json;
using AMR.Financeiro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AMR.Financeiro.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Exchange = "lancamentos";
    private readonly object _lock = new();

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private IModel? GetChannel()
    {
        if (_channel is { IsOpen: true }) return _channel;

        lock (_lock)
        {
            if (_channel is { IsOpen: true }) return _channel;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                    Port     = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = _configuration["RabbitMQ:User"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest",
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
                };
                _connection = factory.CreateConnection();
                _channel    = _connection.CreateModel();
                _channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, durable: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ indisponivel: {Msg}", ex.Message);
                return null;
            }
        }
        return _channel;
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var channel = GetChannel();
        if (channel is null)
        {
            _logger.LogWarning("Evento {Tipo} nao publicado — RabbitMQ offline.", typeof(T).Name);
            return Task.CompletedTask;
        }

        var json  = JsonSerializer.Serialize(@event);
        var body  = Encoding.UTF8.GetBytes(json);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        var queue = "lancamento.criado";
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, Exchange, string.Empty);
        channel.BasicPublish(Exchange, string.Empty, props, body);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
