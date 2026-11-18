using Newtonsoft.Json;
using TrackAdmin.Models;

namespace TrackAdmin.Data
{
    public class RiderTracking
    {
        private readonly HttpClient http;

        public RiderTracking(HttpClient httpClient)
        {
            http = httpClient;
        }

        public async Task<(double Distance, double Time)> GetMapRoute(double slat, double slon, double elat, double elon)
        {
            string accessToken = "YOUR_ACTUAL_SECRET_TOKEN";
            string start = $"{slon},{slat}";
            string end = $"{elon},{elat}";

            var response = await http.GetAsync(
                $"https://api.mapbox.com/directions/v5/mapbox/driving/{start};{end}?access_token={accessToken}");

            double distance = 0;
            double time = 0;

            if (response.IsSuccessStatusCode)
            {
                var tt = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(tt);

                if (json?.routes != null && json.routes.Count > 0)
                {
                    distance = json.routes[0].distance;
                    time = json.routes[0].duration;
                }
            }

            return (Math.Round(distance / 1000, 2), time);
        }

        public string FormatDuration(double seconds)
        {
            int totalMinutes = (int)(seconds / 60);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (hours > 0)
                return $"{hours} hr {minutes} min";
            else
                return $"{minutes} min";
        }

        public async Task<IList<Rider_Current_Location>> GetRCL()
        {
            var response = await http.GetAsync("LiveTracker/RiderCurrent");

            if (!response.IsSuccessStatusCode)
                return new List<Rider_Current_Location>();

            var tt = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IList<Rider_Current_Location>>(tt)
                   ?? new List<Rider_Current_Location>();
        }

        public void TrimAllStrings(object obj)
        {
            if (obj == null)
                return;

            var stringProps = obj.GetType()
                                 .GetProperties()
                                 .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

            foreach (var prop in stringProps)
            {
                var value = prop.GetValue(obj) as string;
                if (value != null)
                {
                    prop.SetValue(obj, value.Trim());
                }
            }
        }
    }
}