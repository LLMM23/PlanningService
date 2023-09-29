using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace ServiceWorker;

public class Worker : BackgroundService
{
    private string _workPath = string.Empty;
    private readonly ILogger<Worker> _logger;

    /*
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        // <indsæt noget RabbitMQ connectionfactory kode her!>
        var factory = new ConnectionFactory { HostName = "localhost" }; //Ændring 

    }
    */

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _workPath = configuration["WorkPath"] ?? String.Empty;
        var factory = new ConnectionFactory { HostName = "localhost" }; //Husk ændring 

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "hello",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);


        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            //PlanDTO - Skal til passes
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
        };
        channel.BasicConsume(queue: "hello",
                            autoAck: true,
                            consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            // 1. <indsæt noget RabbitMQ Query + serialiserings kode her!>
            // 2. <Tilføj BookingDTO fra køen til lokal Repository-klasse!>
            await Task.Delay(1000, stoppingToken);
        }

    }
    private void WriteToCsv(string message) //Skriver modtaget instanser til plan.csv fil
    {
        //Tjekker først om "_workPatch" eksisterer
        if (!Directory.Exists(_workPath))
        {
            Directory.CreateDirectory(_workPath);
        }

        //Definerer stien/path for vores csv-fil, hvilket er plan.csv
        var filePath = Path.Combine(_workPath, "plan.csv");


        //Skriver beskeden til csv-filen
        //Dette skal tilpasses den format af vores PlanDTO
        using (var writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(message);
        }
    }



}
