using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class DashboardService
    {
        private readonly HttpClient _http;

        public DashboardService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DashboardFullResponse?> GetFullDashboardAsync(int trendDays = 30, string? fromDate = null, string? toDate = null)
        {
            try
            {
                var url = $"api/dashboard/full?trendDays={trendDays}";
                if (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(toDate))
                {
                    url += $"&fromDate={fromDate}&toDate={toDate}";
                }
                return await _http.GetFromJsonAsync<DashboardFullResponse>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard full data: {ex.Message}");
                return null;
            }
        }
    }
}
