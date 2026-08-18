using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Booking.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Booking.Services
{
    public class ReportService
    {
        private readonly HttpClient _http;

        public ReportService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Fetches IP revenue report from api/report/getip?fromdate=yyyy-MM-dd&todate=yyyy-MM-dd
        /// </summary>
        public async Task<List<ReportRevenueItem>> GetIpReportAsync(DateTime fromDate, DateTime toDate)
        {
            var fromStr = fromDate.ToString("yyyy-MM-dd");
            var toStr = toDate.ToString("yyyy-MM-dd");
            var url = $"api/report/getip?fromdate={fromStr}&todate={toStr}";
            return await FetchReportItemsAsync(url, isIp: true);
        }

        /// <summary>
        /// Fetches OP revenue report from api/report/getop?fromdate=yyyy-MM-dd&todate=yyyy-MM-dd
        /// </summary>
        public async Task<List<ReportRevenueItem>> GetOpReportAsync(DateTime fromDate, DateTime toDate)
        {
            var fromStr = fromDate.ToString("yyyy-MM-dd");
            var toStr = toDate.ToString("yyyy-MM-dd");
            var url = $"api/report/getop?fromdate={fromStr}&todate={toStr}";
            return await FetchReportItemsAsync(url, isIp: false);
        }

        /// <summary>
        /// Combined fetch for both IP and OP revenue reports and daily breakdown
        /// </summary>
        public async Task<CombinedRevenueReport> GetCombinedRevenueReportAsync(DateTime fromDate, DateTime toDate)
        {
            // Ensure fromDate <= toDate
            if (fromDate > toDate)
            {
                var temp = fromDate;
                fromDate = toDate;
                toDate = temp;
            }

            var ipTask = GetIpReportAsync(fromDate, toDate);
            var opTask = GetOpReportAsync(fromDate, toDate);

            await Task.WhenAll(ipTask, opTask);

            var ipItems = await ipTask;
            var opItems = await opTask;

            var report = new CombinedRevenueReport
            {
                IpItems = ipItems,
                OpItems = opItems,
                TotalIpRevenue = ipItems.Sum(i => i.NetAmount > 0 ? i.NetAmount : i.PaidAmount),
                TotalOpRevenue = opItems.Sum(o => o.NetAmount > 0 ? o.NetAmount : o.PaidAmount),
                TotalIpCount = ipItems.Count,
                TotalOpCount = opItems.Count
            };

            // Build daily summary list covering all dates in range
            var dailyMap = new Dictionary<DateTime, DailyRevenueSummary>();
            for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
            {
                dailyMap[d] = new DailyRevenueSummary
                {
                    Date = d,
                    DateLabel = d.ToString("dd MMM yyyy")
                };
            }

            foreach (var item in ipItems)
            {
                var d = item.Date.Date;
                if (!dailyMap.TryGetValue(d, out var summary))
                {
                    summary = new DailyRevenueSummary
                    {
                        Date = d,
                        DateLabel = d.ToString("dd MMM yyyy")
                    };
                    dailyMap[d] = summary;
                }
                var amt = item.NetAmount > 0 ? item.NetAmount : item.PaidAmount;
                summary.IpRevenue += amt;
                summary.IpTransactionCount++;
            }

            foreach (var item in opItems)
            {
                var d = item.Date.Date;
                if (!dailyMap.TryGetValue(d, out var summary))
                {
                    summary = new DailyRevenueSummary
                    {
                        Date = d,
                        DateLabel = d.ToString("dd MMM yyyy")
                    };
                    dailyMap[d] = summary;
                }
                var amt = item.NetAmount > 0 ? item.NetAmount : item.PaidAmount;
                summary.OpRevenue += amt;
                summary.OpTransactionCount++;
            }

            report.DailySummaries = dailyMap.Values.OrderBy(x => x.Date).ToList();

            var today = DateTime.Today;
            if (dailyMap.TryGetValue(today, out var todaySummary))
            {
                report.TodayIpRevenue = todaySummary.IpRevenue;
                report.TodayOpRevenue = todaySummary.OpRevenue;
            }

            return report;
        }

        private async Task<List<ReportRevenueItem>> FetchReportItemsAsync(string url, bool isIp)
        {
            var items = new List<ReportRevenueItem>();
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ReportService] GET {url} status code: {response.StatusCode}");
                    return items;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(rawJson))
                    return items;

                rawJson = rawJson.Trim();

                // Strip UTF-8 BOM if present
                if (rawJson.StartsWith("\uFEFF"))
                {
                    rawJson = rawJson.Substring(1).Trim();
                }

                // If response is a JSON string wrapped in double quotes (double-serialized JSON string), unwrap it
                if (rawJson.StartsWith("\"") && rawJson.EndsWith("\""))
                {
                    try
                    {
                        var unwrapped = JsonConvert.DeserializeObject<string>(rawJson);
                        if (!string.IsNullOrWhiteSpace(unwrapped))
                        {
                            rawJson = unwrapped.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ReportService] String unwrap notice: {ex.Message}");
                    }
                }

                Console.WriteLine($"[ReportService] Raw JSON snippet ({url}): {(rawJson.Length > 200 ? rawJson.Substring(0, 200) : rawJson)}");

                if (!rawJson.StartsWith("[") && !rawJson.StartsWith("{"))
                {
                    Console.WriteLine($"[ReportService] Raw content is not JSON object/array: {rawJson}");
                    return items;
                }

                var token = JToken.Parse(rawJson);

                if (token is JArray jArray)
                {
                    foreach (var elem in jArray)
                    {
                        if (elem is JObject jObj)
                        {
                            var item = ParseJObject(jObj, isIp);
                            if (item != null) items.Add(item);
                        }
                    }
                }
                else if (token is JObject jObj)
                {
                    JArray? arrayElem = null;
                    foreach (var prop in new[] { "value", "data", "items", "records", "result", "list" })
                    {
                        if (jObj[prop] is JArray arr)
                        {
                            arrayElem = arr;
                            break;
                        }
                    }

                    if (arrayElem != null)
                    {
                        foreach (var elem in arrayElem)
                        {
                            if (elem is JObject obj)
                            {
                                var item = ParseJObject(obj, isIp);
                                if (item != null) items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        var item = ParseJObject(jObj, isIp);
                        if (item != null) items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReportService] Error parsing report response from {url}: {ex.Message}");
            }

            return items;
        }

        private ReportRevenueItem? ParseJObject(JObject jObj, bool isIp)
        {
            var item = new ReportRevenueItem { IsIp = isIp };

            // Parse Date
            item.Date = GetDateProp(jObj, "date", "billdate", "createddate", "transdate", "admitdate", "receiptdate", "bill_date", "visitdate", "todate", "fromdate") ?? DateTime.Today;

            // Parse Bill / Transaction ID
            item.BillNo = GetStringProp(jObj, "billno", "ipno", "opno", "bill_no", "ip_no", "op_no", "recno", "receipt_no", "billnumber", "invoice_no", "id") ?? "";

            // Parse Patient Name
            item.PatientName = GetStringProp(jObj, "patient_name", "custname", "patientname", "name", "cust_name", "patient", "custid") ?? "Patient";

            // Parse Doctor Name
            item.DoctorName = GetStringProp(jObj, "doctor_name", "docname", "doctorname", "dname", "doctor", "doc_name") ?? "";

            // Parse Category / Department
            item.Category = GetStringProp(jObj, "category", "department", "feetype", "service", "type", "description") ?? (isIp ? "IP Revenue" : "OP Revenue");

            // Parse Amounts
            item.TotalAmount = GetDecimalProp(jObj, "totalamount", "amount", "total_amount", "grossamount", "gross_amount", "billamount") ?? 0m;
            item.Discount = GetDecimalProp(jObj, "discount", "discountamount", "disc") ?? 0m;
            item.Tax = GetDecimalProp(jObj, "tax", "gst", "taxamount") ?? 0m;
            item.NetAmount = GetDecimalProp(jObj, "netamount", "net_amount", "grandtotal", "total", "net") ?? (item.TotalAmount - item.Discount + item.Tax);
            item.PaidAmount = GetDecimalProp(jObj, "paidamount", "paid_amount", "received", "collected", "paid") ?? item.NetAmount;
            item.DueAmount = GetDecimalProp(jObj, "dueamount", "due_amount", "balance", "pending") ?? Math.Max(0, item.NetAmount - item.PaidAmount);

            // Parse PayMode
            item.PayMode = GetStringProp(jObj, "paymode", "payment_mode", "pay_mode", "paymodename", "mode") ?? "Cash";

            // Parse Status
            item.Status = GetStringProp(jObj, "status", "billstatus", "state") ?? "COMPLETED";

            return item;
        }

        private string? GetStringProp(JObject jObj, params string[] names)
        {
            foreach (var name in names)
            {
                var prop = jObj.Properties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.Value != null && prop.Value.Type != JTokenType.Null)
                {
                    return prop.Value.ToString();
                }
            }
            return null;
        }

        private decimal? GetDecimalProp(JObject jObj, params string[] names)
        {
            foreach (var name in names)
            {
                var prop = jObj.Properties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.Value != null && prop.Value.Type != JTokenType.Null)
                {
                    if (decimal.TryParse(prop.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    {
                        return val;
                    }
                }
            }
            return null;
        }

        private DateTime? GetDateProp(JObject jObj, params string[] names)
        {
            foreach (var name in names)
            {
                var prop = jObj.Properties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.Value != null && prop.Value.Type != JTokenType.Null)
                {
                    var valStr = prop.Value.ToString();
                    if (DateTime.TryParse(valStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        return dt;
                    }
                }
            }
            return null;
        }
    }
}
