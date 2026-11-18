using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeZoneConverter;
using TrackAdmin.Models;


namespace TrackAdmin.Data
{
    internal class MasterData
    {
        HttpClient http;
       
        public MasterData(HttpClient _http)
        {
            http = _http;
           
        }
       
        public async Task<IList<T>> GetMasters<T>(string api)
        {
            try
            {
                var response1 = await http.GetAsync(api);
                var tt = await response1.Content.ReadAsStringAsync();
               var  refm = JsonConvert.DeserializeObject<IList<T>>(tt);
                return refm?? new List<T>();
            }
            catch (Exception ex)
            {
               return new List<T>();
            }
        }

        public async Task<TimeZoneInfo> ConvertTime(string dt)
        {
            // Let's say you got this from the client:
            string userIanaTimeZone = dt; // or whatever the browser sent

            // Convert IANA to Windows time zone
            string windowsTz = TZConvert.IanaToWindows(userIanaTimeZone);

            // Now get TimeZoneInfo
            TimeZoneInfo userZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTz);

            return userZoneInfo;
        }
        public async Task<IList<RiderStatus>> GetRiderStatus(string apiurl, int id, string sd, string ed)
        {
            GetRiderStatusQuery grd = new GetRiderStatusQuery() 
            { 
                RiderID=id,
                fd=sd,
                td=ed
            };

            var res =await http.PostAsJsonAsync($"{apiurl}LiveTracker/GetStatusByID", grd);
            if (!res.IsSuccessStatusCode)
            {
                return new List<RiderStatus>();
            }

            var tt = res.Content.ReadAsStringAsync().Result;
            var rs = JsonConvert.DeserializeObject<IList<RiderStatus>>(tt);
            return rs;

        }

        public async Task<IList<SampleCollection_Master>> GetSampleCollection(string apiurl, int rid, string sd, string ed)
        {
           

            var res = await http.GetAsync($"{apiurl}SampleCollection/CollectionView?fd={Uri.EscapeUriString(sd)}&td={Uri.EscapeUriString(ed)}&rid={rid}");
            if (!res.IsSuccessStatusCode)
            {
                return new List<SampleCollection_Master>();
            }

            var tt = res.Content.ReadAsStringAsync().Result;
            var rs = JsonConvert.DeserializeObject<IList<SampleCollection_Master>>(tt);
            return rs;

        }



        public async Task<(IList<Appointments>, IList<Referral_Master>)> GetAppList(int id, string apiurl, string sd, string ed)
        {
            try
            {
                string url = "";
                if (id == 1)
                {
                    url = $"{apiurl}sampleCollection/appdate?sd={Uri.EscapeUriString(sd)}&ed={Uri.EscapeUriString(ed)}&isupcoming={false}";
                }
                else if (id == 2)
                {
                    url = $"{apiurl}sampleCollection/appdate?sd={Uri.EscapeUriString(sd)}&ed={Uri.EscapeUriString(ed)}&isupcoming={true}";
                }

                var res = await http.GetAsync(url);
                if (!res.IsSuccessStatusCode) return (new List<Appointments>(), new List<Referral_Master>());

                var tt = await res.Content.ReadAsStringAsync();
                IList<Appointments> applist1 = JsonConvert.DeserializeObject<IList<Appointments>>(tt);
                IList<Appointments> applist = applist1.Where(x => x.Deleted == false).ToList();
                IList<Referral_Master> rm1 = new List<Referral_Master>();
                if (applist.Count > 0)
                {
                    string rid = string.Join(",", applist.Select(x => $"{x.ReferralID}"));
                    var res1 = await http.GetAsync($"{apiurl}referral/referralid?rname={Uri.EscapeUriString(rid)}");
                    if (res1.IsSuccessStatusCode)
                    {
                        var tt1 = await res1.Content.ReadAsStringAsync();
                        rm1 = JsonConvert.DeserializeObject<IList<Referral_Master>>(tt1);
                    }
                }

                return (applist, rm1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAppList: {ex.Message}");
                return (new List<Appointments>(), new List<Referral_Master>());
            }
        }

        public IList<RiderVisit> CalculateRiderVisits(IList<RiderStatus> riderStatuses, IList<Referral_Master> referrals)
        {
            IList<RiderVisit> visits = new List<RiderVisit>();

            foreach (var rider in riderStatuses.GroupBy(r => r.RiderID))
            {
                foreach (var referral in referrals)
                {
                    DateTime? inTime = null;
                    DateTime? outTime = null;

                    foreach (var status in rider.OrderBy(r => r.StartTime))
                    {
                        double distance = CalculateDistance(
                            double.Parse(status.latitude),
                            double.Parse(status.longitude),
                            referral.Latitude,
                            referral.Longitude
                        );

                        if (distance <= referral.Radius)
                        {
                            if (inTime == null) inTime = status.StartTime;
                            outTime = status.EndTime;
                        }
                        else if (inTime != null)
                        {
                            visits.Add(new RiderVisit
                            {
                                RiderID = status.RiderID,
                                ReferralID = referral.ReferralID,
                                InTime = inTime.Value,
                                OutTime = outTime ?? status.EndTime,
                                TotalMinutes = ((outTime ?? status.EndTime) - inTime.Value).TotalMinutes
                            });

                            inTime = null;
                            outTime = null;
                        }
                    }

                    if (inTime != null)
                    {
                        visits.Add(new RiderVisit
                        {
                            RiderID = rider.Key,
                            ReferralID = referral.ReferralID,
                            InTime = inTime.Value,
                            OutTime = outTime ?? inTime.Value,
                            TotalMinutes = ((outTime ?? inTime.Value) - inTime.Value).TotalMinutes
                        });
                    }
                }
            }

            return visits;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Earth radius in meters
            var φ1 = lat1 * Math.PI / 180;
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }



        public DateTime ConvertToUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified as UTC
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            return dateTime.ToUniversalTime();
        }

        // Converts DateTime to specified timezone
        public DateTime ConvertToTimeZone(DateTime utcDateTime, string timeZoneId)
        {
            try 
            {
                // Use TZConvert to ensure compatibility between Windows (Arabian Standard Time) and IANA (Asia/Dubai)
                string platformTzId = TZConvert.IanaToWindows(timeZoneId);
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(platformTzId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
            }
            catch (Exception ex)
            {
                // Fallback to UTC if timezone is missing on this OS
                Console.WriteLine($"TimeZone Error: {timeZoneId} not found. {ex.Message}");
                return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
        }
        public DateTime GetDubaiTime(DateTime utcDateTime)
        {
            var tt = ConvertToUtc(utcDateTime);
            return ConvertToTimeZone(tt, "Asia/Dubai");
        }
        public DateTime GetIndianTime(DateTime utcDateTime)
        {
            var tt = ConvertToUtc(utcDateTime);
            return ConvertToTimeZone(tt, "Asia/Kolkata");
        }


        // Converts DateTime to Dubai time (for convenience)
        public DateTime ConvertToDubaiTime(DateTime utcDateTime)
        {

            return ConvertToTimeZone(utcDateTime, "Asia/Dubai");
        }

        // Converts DateTime to India time (for convenience)
        public DateTime ConvertToIndiaTime(DateTime utcDateTime)
        {
            return ConvertToTimeZone(utcDateTime, "Asia/Kolkata");
        }


        

        public async Task<IList<SampleTransferResponse>> GetSampleTransfers(string apiurl, string sd, string ed, int rid, string status = "")
        {
            try
            {
                var res = await http.GetAsync($"{apiurl}SampleCollection/GetTransfers?sd={Uri.EscapeUriString(sd)}&ed={Uri.EscapeUriString(ed)}&riderId={rid}&status={status}");
                if (!res.IsSuccessStatusCode)
                {
                    return new List<SampleTransferResponse>();
                }

                var tt = await res.Content.ReadAsStringAsync();
                var rs = JsonConvert.DeserializeObject<IList<SampleTransferResponse>>(tt);
                return rs ?? new List<SampleTransferResponse>();
            }
            catch (Exception ex)
            {
                return new List<SampleTransferResponse>();
            }
        }
        public async Task<(bool Success, string Message)> ReassignTransfer(string apiurl, int transferId, int newRiderId)
        {
            try
            {
                var payload = new { transferId = transferId, newRiderId = newRiderId };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var res = await http.PostAsync($"{apiurl}SampleCollection/ReassignTransfer", content);
                var responseContent = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ReassignTransfer Failed: {res.StatusCode} - {responseContent}");
                    try {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return (false, (string)errorObj?.Message ?? "Server error");
                    } catch {
                        return (false, $"Error: {res.StatusCode}");
                    }
                }
                
                return (true, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReassignTransfer Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }
}
