using System;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AkkaPlayground
{
    internal static class Program
    {
        private static async Task Main()
        {
            using var webSocket = new ClientWebSocket();
            using var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://192.168.88.203:9080/api/84594D24F2/")
            };
            
            
            var cancellationTokenSource = new CancellationTokenSource();
            
            await webSocket.ConnectAsync(new Uri("ws://192.168.88.203:443"), cancellationTokenSource.Token);

            while (true)
            {
                var buffer = new byte[2048];
                var memory = new Memory<byte>(buffer);
                var receiveResult = await webSocket.ReceiveAsync(memory, cancellationTokenSource.Token);
                
                var message = Encoding.UTF8.GetString(buffer.Take(receiveResult.Count).ToArray());
                
                var document = JsonDocument.Parse(message);

                var messageType = document.RootElement.GetProperty("t").GetString();
                var eventType = document.RootElement.GetProperty("e").GetString();
                var resourceType = document.RootElement.GetProperty("r").GetString();
                var resourceId = document.RootElement.GetProperty("id").GetString();

                if (messageType != "event" || eventType != "changed" || resourceType != "sensors" ||
                    resourceId != "9") continue;
                
                if (!document.RootElement.TryGetProperty("state", out var stateDocument)) continue;
                    
                var buttonEvent = stateDocument.GetProperty("buttonevent").GetInt64();
                if (buttonEvent == 1002)
                {
                    await httpClient.PutAsync("lights/15/state", new StringContent("{ \"on\": false }", Encoding.UTF8), cancellationTokenSource.Token);
                }
                else
                {
                    await httpClient.PutAsync("lights/15/state", new StringContent("{ \"on\": true }", Encoding.UTF8), cancellationTokenSource.Token);
                }
                
                Console.WriteLine(buttonEvent);

                if (cancellationTokenSource.Token.IsCancellationRequested) break;
            }
        }
    }
}