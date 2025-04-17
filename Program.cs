using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;


/*
dotnet add package Azure.Messaging.EventHubs
dotnet add package Azure.Messaging.EventHubs.Producer
*/

class Program
{
    private const string connectionString = "Endpoint=sb://ehr-vitals-icu.servicebus.windows.net/;SharedAccessKeyName=access;SharedAccessKey=M7faZqnclsi6R6IKMicp3CPzT+nh2MCKM+AEhEU/OW4=;EntityPath=patient-vitals-eh";
    private const string eventHubName = "patient-vitals-eh";
    private static readonly Random random = new();

    static async Task Main()
    {
        Console.WriteLine("Sending patient vitals to Event Hub... Press Ctrl+C to stop.");

        await using var producerClient = new EventHubProducerClient(connectionString, eventHubName);

        while (true)
        {
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            for (int icu = 1; icu <= 12; icu++)
            {
                var vitals = GenerateRandomVitals(icu);
                var json = JsonSerializer.Serialize(vitals);
                Console.WriteLine($"Sent: {json}");

                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json))))
                {
                    Console.WriteLine($"Warning: Could not add ICU-{icu} event to batch.");
                }
            }

            await producerClient.SendAsync(eventBatch);
            await Task.Delay(10000); // Wait 10 seconds before sending the next full batch
        }
    }

    static object GenerateRandomVitals(int icuNumber)
    {
        int systolic = random.Next(90, 140);
        int diastolic = random.Next(60, 90);

        return new
        {
            icuUnit = $"ICU-{icuNumber}",
            heartRate = random.Next(60, 100),
            systolicBP = systolic,
            diastolicBP = diastolic,
            temperature = Math.Round(random.NextDouble() * (39.0 - 36.5) + 36.5, 1),
            oxygenSaturation = random.Next(95, 100),
            timestamp = DateTime.UtcNow
        };
    }
}

