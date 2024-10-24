using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NRedisStack;
using StackExchange.Redis;

namespace Quickie
{
    public struct JsonEvent
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("flavor")]
        public string Flavor { get; set; }
        [JsonPropertyName("elapsed")]
        public double Elapsed { get; set; }
        [JsonPropertyName("md5")]
        public string Md5 { get; set; }
    }

    public class Program
    {
        private static readonly MD5 _md5 = MD5.Create();

        public static string Base64StringToMd5(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return String.Join("", _md5.ComputeHash(base64EncodedBytes).Select(x => x.ToString("x2")));
        }

        static async Task Main(string[] args)
        {

            string RedisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{RedisHost}:6379");
            IDatabase db = redis.GetDatabase();

            string InputQueue = Environment.GetEnvironmentVariable("INPUT_QUEUE") ?? "quickie";
            string OutputQueue = Environment.GetEnvironmentVariable("OUTPUT_QUEUE") ?? "quickie-out";
            double Timeout = double.Parse(Environment.GetEnvironmentVariable("TIMEOUT") ?? "0");

            JsonEvent @event; // Declare event variable
            Stopwatch stopWatch = new();

            while (true)
            {
                var blpopResult = await db.BLPopAsync(InputQueue, Timeout);
                if (blpopResult == null)
                {
                    continue;
                }
                stopWatch.Start();
                var (_, element) = blpopResult;
                if (element.HasValue)
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(element.ToString())))
                    {
                        @event = await JsonSerializer.DeserializeAsync<JsonEvent>(ms);
                    }
                    @event.Flavor = "dotnet";
                    @event.Md5 = Base64StringToMd5(@event.File);
                    stopWatch.Stop();
                    @event.Elapsed = stopWatch.Elapsed.TotalMilliseconds;
                    stopWatch.Reset();
                    Console.WriteLine(JsonSerializer.Serialize(@event));
                    await db.ListLeftPushAsync(OutputQueue, JsonSerializer.Serialize(@event));
                }
            }
        }
    }
}
