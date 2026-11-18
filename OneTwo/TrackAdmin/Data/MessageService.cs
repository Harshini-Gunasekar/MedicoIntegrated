using Microsoft.AspNetCore.SignalR.Client;

namespace TrackAdmin.Data
{
    public class MessageService
    {
        private HubConnection _hubConnection;
        public event Action<List<string>>? OnClientsUpdated;

        public List<string> Incoming { get; private set; } = new();
        public event Action<List<string>>? OnReceiveNote;
        public List<string> ConnectedClients { get; private set; } = new();

        public async Task ConnectAsync(string token, string baseUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl.TrimEnd('/')}/notificationHub", options =>
                {
                    options.Headers["Device-Id"] = "Web"; // Send device ID
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .Build();

            // Listen for the connection ID
            _hubConnection.On<string>("ReceiveRiderLocation", (id) =>
            {
                Incoming.Add(id);
                OnReceiveNote?.Invoke((Incoming));
            });

            // Listen for the updated client list
            _hubConnection.On<List<string>>("UpdateConnectedClients", (clients) =>
            {
                ConnectedClients = clients;
                OnClientsUpdated?.Invoke(clients);
            });

            await _hubConnection.StartAsync();

            if (_hubConnection.State == HubConnectionState.Connected && _hubConnection.ConnectionId != null)
            {
               // await _hubConnection.InvokeAsync("SendMessage", , "Welcome Sir");
            }
        }

        public async Task<List<string>> GetConnectedClientsAsync()
        {
            int maxRetries = 3; // Number of retry attempts
            int delayMs = 2000; // Delay in milliseconds

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (_hubConnection.State != HubConnectionState.Connected)
                {
                    try
                    {
                        await _hubConnection.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Retry {attempt + 1}/{maxRetries} - Connection failed: {ex.Message}");
                        await Task.Delay(delayMs); // Wait before retrying
                    }
                }

                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    return await _hubConnection.InvokeAsync<List<string>>("GetConnectedClients");
                }
            }

            return new List<string>(); // Return empty list if retries fail
        }

        public async Task SendMessageToClient(string clientId, string message, bool fal)
        {
            if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendNotification", clientId, message, fal);
            }
        }
        public async Task<string> ReceiveMessageFromClient(string msg)
        {
           
            return msg;
        }
    }
}
