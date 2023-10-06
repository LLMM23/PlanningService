using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ServiceWorker
{
    public class Worker : BackgroundService
    {
        private string _workPath = string.Empty;
        private string _mqHost = string.Empty;

        private readonly ILogger<Worker> _logger;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _workPath = configuration["WorkPath"] ?? string.Empty;
            _mqHost = configuration["MqHost"] ?? string.Empty;
            _logger.LogInformation($"env: {_workPath}, {_mqHost}");
        

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = _mqHost }; //husk at ændre
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "plan",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                // PlanDTO - Skal til passes
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                PlanDTO planDTO = JsonSerializer.Deserialize<PlanDTO>(message);

                WriteToCsv(planDTO);
            };
            channel.BasicConsume(queue: "plan",
                                autoAck: true,
                                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now); #Midlertidig kommenteret ud
                // 1. <Insert RabbitMQ Query + serialization code here!>
                // 2. <Tilføj BookingDTO fra køen til lokal Repository-klasse!>
                await Task.Delay(1000, stoppingToken);
            }

        }
        private void WriteToCsv(PlanDTO message) // Skriver modtaget instanser til plan.csv fil
        {
            // Tjekker først om "_workPatch" eksisterer
            if (!Directory.Exists(_workPath))
            {
                Directory.CreateDirectory(_workPath);
            }

            // Definerer stien/path for vores csv-fil, hvilket er plan.csv
            var filePath = Path.Combine(_workPath, "Plan.csv");

            // Analyser den modtagne besked og opret en PlanDTO-instans
            var plan = ParseMessageToPlanDto(message);

            // Skriver PlanDTO instans til CSV-filen
            using (var writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(plan);
            }
        }
        // Metode til at analysere den modtagne besked og oprette en PlanDTO-instans
        private string ParseMessageToPlanDto(PlanDTO plan)
        {
            var csvline = $"{plan.CustomerName},{plan.StartTime},{plan.StartLocation},{plan.EndLocation}";
            return csvline;
        }
    }
}



