using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

namespace Booking.Services
{
    public class PatientUhidService
    {
        private readonly string _connectionString;
        private readonly IHttpClientFactory _clientFactory;

        // In-memory caches for ultra-fast instant lookups without DB roundtrips
        private static readonly ConcurrentDictionary<(string tenant, decimal custid), string> _uhidCache = new();
        private static readonly ConcurrentDictionary<(string tenant, string mobile), string> _mobileToUhidMap = new();
        private static readonly ConcurrentDictionary<(string tenant, string name), string> _nameToUhidMap = new();
        private static readonly ConcurrentDictionary<string, bool> _preloadedTenants = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, byte> _loadingTenants = new(StringComparer.OrdinalIgnoreCase);
        private static readonly SemaphoreSlim _dbSemaphore = new(2, 2);

        public PatientUhidService(IConfiguration config, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;

            var rawConn = config.GetConnectionString("conn") 
                ?? "Host=103.108.220.19;Port=5432;Database=medico;Username=postgres;Password=wafewin;";

            var csb = new NpgsqlConnectionStringBuilder(rawConn)
            {
                Pooling = true,
                MaxPoolSize = 4,
                MinPoolSize = 0,
                ConnectionIdleLifetime = 10,
                Timeout = 6,
                CommandTimeout = 6
            };
            _connectionString = csb.ConnectionString;
        }

        public static string? CleanUhid(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var clean = code.Trim();
            if (clean.StartsWith("CUST-", StringComparison.OrdinalIgnoreCase) ||
                clean.StartsWith("CUST", StringComparison.OrdinalIgnoreCase) ||
                clean.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                decimal.TryParse(clean, out _))
            {
                return null;
            }
            return clean;
        }

        /// <summary>
        /// Fast synchronous lookup. Returns the configured tenant custcode if available, or clean directCustCode.
        /// NEVER returns a raw numeric custid or CUST- prefix.
        /// </summary>
        public string GetUhid(decimal custid, string? directCustCode = null, string? tenantCode = null)
        {
            var validDirect = CleanUhid(directCustCode);
            if (!string.IsNullOrWhiteSpace(validDirect))
            {
                if (custid > 0)
                {
                    _uhidCache[(tenantCode ?? "", custid)] = validDirect;
                    _uhidCache[("", custid)] = validDirect;
                }
                return validDirect;
            }

            if (custid <= 0) return "-";

            string keyTenant = tenantCode ?? "";
            if (_uhidCache.TryGetValue((keyTenant, custid), out var cached) && !string.IsNullOrWhiteSpace(cached))
            {
                var cleanCached = CleanUhid(cached);
                if (!string.IsNullOrWhiteSpace(cleanCached)) return cleanCached;
            }

            if (!string.IsNullOrEmpty(keyTenant) && _uhidCache.TryGetValue(("", custid), out var cachedDefault) && !string.IsNullOrWhiteSpace(cachedDefault))
            {
                var cleanDefault = CleanUhid(cachedDefault);
                if (!string.IsNullOrWhiteSpace(cleanDefault)) return cleanDefault;
            }

            // Trigger single ID background load if not cached yet
            _ = Task.Run(async () => await PreloadUhidsAsync(new[] { custid }, keyTenant));

            return !string.IsNullOrWhiteSpace(validDirect) ? validDirect : "-";
        }

        public string GetUhid(decimal? custid, string? directCustCode = null, string? tenantCode = null)
        {
            if (custid == null || custid.Value <= 0)
            {
                var clean = CleanUhid(directCustCode);
                return !string.IsNullOrWhiteSpace(clean) ? clean : "-";
            }
            return GetUhid(custid.Value, directCustCode, tenantCode);
        }

        public async Task<string> GetUhidAsync(decimal custid, string? directCustCode = null, string? tenantCode = null)
        {
            var validDirect = CleanUhid(directCustCode);
            if (!string.IsNullOrWhiteSpace(validDirect))
            {
                if (custid > 0)
                {
                    _uhidCache[(tenantCode ?? "", custid)] = validDirect;
                    _uhidCache[("", custid)] = validDirect;
                }
                return validDirect;
            }

            if (custid <= 0) return "-";

            string keyTenant = tenantCode ?? "";
            if (_uhidCache.TryGetValue((keyTenant, custid), out var cached) && !string.IsNullOrWhiteSpace(cached))
            {
                var cleanCached = CleanUhid(cached);
                if (!string.IsNullOrWhiteSpace(cleanCached)) return cleanCached;
            }

            await PreloadUhidsAsync(new[] { custid }, keyTenant);

            if (_uhidCache.TryGetValue((keyTenant, custid), out var loaded) && !string.IsNullOrWhiteSpace(loaded))
            {
                var cleanLoaded = CleanUhid(loaded);
                if (!string.IsNullOrWhiteSpace(cleanLoaded)) return cleanLoaded;
            }

            return "-";
        }

        public async Task EnsureTenantPreloadedAsync(string? tenantCode)
        {
            if (string.IsNullOrWhiteSpace(tenantCode)) return;
            string keyTenant = tenantCode.Trim();

            if (_preloadedTenants.ContainsKey(keyTenant)) return;
            if (!_loadingTenants.TryAdd(keyTenant, 0)) return;

            try
            {
                if (!await _dbSemaphore.WaitAsync(2000)) return;
                try
                {
                    using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync();

                    const string sql = @"
                    SELECT DISTINCT ON (crm.custid) crm.custid, crm.custcode, crm.tenant_code, cm.mobile, cm.name
                    FROM customerdb.customer_registration_master crm
                    LEFT JOIN customerdb.customer_master cm ON cm.custid = crm.custid
                    WHERE (crm.tenant_code = @tenant OR @tenant = '') AND crm.custcode IS NOT NULL AND crm.custcode <> ''
                    ORDER BY crm.custid, (CASE WHEN crm.tenant_code = @tenant THEN 0 ELSE 1 END), crm.regid DESC";

                    var rows = await conn.QueryAsync(sql, new { tenant = keyTenant });
                    foreach (var row in rows)
                    {
                        decimal cid = (decimal)row.custid;
                        string? code = CleanUhid(row.custcode?.ToString());
                        if (string.IsNullOrWhiteSpace(code)) continue;

                        _uhidCache[(keyTenant, cid)] = code;
                        _uhidCache[("", cid)] = code;

                        string? mob = row.mobile?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(mob))
                        {
                            _mobileToUhidMap[(keyTenant, mob)] = code;
                            _mobileToUhidMap[("", mob)] = code;
                        }

                        string? name = row.name?.ToString()?.Trim().ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _nameToUhidMap[(keyTenant, name)] = code;
                        }
                    }

                    _preloadedTenants[keyTenant] = true;
                }
                finally
                {
                    _dbSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PatientUhidService] Notice loading tenant customers: {ex.Message}");
            }
            finally
            {
                _loadingTenants.TryRemove(keyTenant, out _);
            }
        }

        public async Task PreloadUhidsAsync(IEnumerable<decimal>? custids, string? tenantCode = null)
        {
            if (custids == null) return;

            string keyTenant = tenantCode ?? "";
            var missingIds = custids
                .Where(id => id > 0 && (!_uhidCache.TryGetValue((keyTenant, id), out var v) || CleanUhid(v) == null))
                .Distinct()
                .Select(id => (long)id)
                .ToArray();

            if (missingIds.Length == 0) return;

            if (!await _dbSemaphore.WaitAsync(2000))
            {
                return;
            }

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
                SELECT DISTINCT ON (crm.custid) crm.custid, crm.custcode, crm.tenant_code
                FROM customerdb.customer_registration_master crm
                WHERE crm.custid = ANY(@custids)
                ORDER BY crm.custid, (CASE WHEN crm.tenant_code = @tenant THEN 0 ELSE 1 END), crm.regid DESC";

                var rows = await conn.QueryAsync(sql, new { tenant = keyTenant, custids = missingIds });
                foreach (var row in rows)
                {
                    decimal cid = (decimal)row.custid;
                    string? uhid = CleanUhid(row.custcode?.ToString());
                    if (!string.IsNullOrWhiteSpace(uhid))
                    {
                        _uhidCache[(keyTenant, cid)] = uhid;
                        _uhidCache[("", cid)] = uhid;
                        if (!string.IsNullOrWhiteSpace(row.tenant_code?.ToString()))
                        {
                            _uhidCache[(row.tenant_code.ToString(), cid)] = uhid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PatientUhidService] Preload notice: {ex.Message}");
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task EnrichCustomersAsync(IEnumerable<SharedComponents.Rcl.Models.CustomerModel>? customers, string? tenantCode = null)
        {
            if (customers == null) return;
            var list = customers.ToList();
            if (!list.Any()) return;

            string keyTenant = tenantCode ?? "";
            var ids = list.Where(c => c.custid > 0).Select(c => (decimal)c.custid).Distinct().ToList();
            if (ids.Any())
            {
                await PreloadUhidsAsync(ids, keyTenant);
                foreach (var c in list)
                {
                    var cleanCode = CleanUhid(c.tenantcustcode) ?? CleanUhid(c.custcode);
                    if (string.IsNullOrWhiteSpace(cleanCode))
                    {
                        var uhid = GetUhid(c.custid, null, keyTenant);
                        cleanCode = CleanUhid(uhid);
                    }
                    if (!string.IsNullOrWhiteSpace(cleanCode))
                    {
                        c.custcode = cleanCode;
                        c.tenantcustcode = cleanCode;
                    }
                }
            }
        }

        public async Task<Dictionary<Guid, string>> GetUhidsByRequestGuidsAsync(IEnumerable<Guid>? guids, string? tenantCode = null)
        {
            var result = new Dictionary<Guid, string>();
            if (guids == null) return result;

            var guidList = guids.Where(g => g != Guid.Empty).Distinct().ToArray();
            if (guidList.Length == 0) return result;

            string keyTenant = tenantCode ?? "";

            if (!await _dbSemaphore.WaitAsync(2000))
            {
                return result;
            }

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
                SELECT m.requestguid, m.custid,
                       COALESCE(
                           (SELECT crm.custcode FROM customerdb.customer_registration_master crm WHERE crm.custid = m.custid AND crm.tenant_code = @tenant LIMIT 1),
                           (SELECT crm.custcode FROM customerdb.customer_registration_master crm WHERE crm.custid = m.custid ORDER BY crm.regid DESC LIMIT 1)
                       ) AS uhid
                FROM lab_request_master m
                WHERE m.requestguid = ANY(@guids)";

                var rows = await conn.QueryAsync(sql, new { tenant = keyTenant, guids = guidList });
                foreach (var row in rows)
                {
                    if (row.requestguid != null)
                    {
                        Guid g = Guid.Parse(row.requestguid.ToString());
                        string? uhid = CleanUhid(row.uhid?.ToString());
                        if (!string.IsNullOrWhiteSpace(uhid))
                        {
                            result[g] = uhid;
                            if (row.custid != null)
                            {
                                decimal cid = (decimal)row.custid;
                                _uhidCache[(keyTenant, cid)] = uhid;
                                _uhidCache[("", cid)] = uhid;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PatientUhidService] Notice GetUhidsByRequestGuidsAsync: {ex.Message}");
            }
            finally
            {
                _dbSemaphore.Release();
            }

            return result;
        }

        public async Task EnrichBillsWithUhidAsync(IEnumerable<Models.HmsBillSummary>? bills, string? tenantCode = null)
        {
            if (bills == null) return;
            var billList = bills.ToList();
            if (!billList.Any()) return;

            string keyTenant = tenantCode ?? "";

            // 1. Fast resolve from existing cache
            foreach (var b in billList)
            {
                var clean = CleanUhid(b.custcode);
                if (!string.IsNullOrWhiteSpace(clean))
                {
                    b.custcode = clean;
                    if (b.custid.HasValue && b.custid.Value > 0)
                    {
                        _uhidCache[(keyTenant, b.custid.Value)] = clean;
                        _uhidCache[("", b.custid.Value)] = clean;
                    }
                }
                else if (b.custid.HasValue && b.custid.Value > 0)
                {
                    if (_uhidCache.TryGetValue((keyTenant, b.custid.Value), out var c1) && CleanUhid(c1) != null)
                        b.custcode = CleanUhid(c1);
                    else if (_uhidCache.TryGetValue(("", b.custid.Value), out var c2) && CleanUhid(c2) != null)
                        b.custcode = CleanUhid(c2);
                }
            }

            var guidsToQuery = billList
                .Where(b => (string.IsNullOrWhiteSpace(b.custcode) || CleanUhid(b.custcode) == null) && !string.IsNullOrWhiteSpace(b.requestguid))
                .Select(b => b.requestguid!)
                .Distinct()
                .ToList();

            var billNosToQuery = billList
                .Where(b => (string.IsNullOrWhiteSpace(b.custcode) || CleanUhid(b.custcode) == null) && !string.IsNullOrWhiteSpace(b.bill_no))
                .Select(b => b.bill_no!.Trim())
                .Distinct()
                .ToList();

            if ((guidsToQuery.Any() || billNosToQuery.Any()) && await _dbSemaphore.WaitAsync(2000))
            {
                try
                {
                    using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync();

                    string whereClause = "";
                    if (guidsToQuery.Any() && billNosToQuery.Any())
                        whereClause = "(m.requestguid::text = ANY(@guids) OR m.requestsnoprint = ANY(@billNos))";
                    else if (guidsToQuery.Any())
                        whereClause = "m.requestguid::text = ANY(@guids)";
                    else
                        whereClause = "m.requestsnoprint = ANY(@billNos)";

                    string sql = $@"
                    SELECT m.requestguid, m.requestsnoprint,
                           m.custid,
                           COALESCE(
                               (SELECT crm.custcode FROM customerdb.customer_registration_master crm WHERE crm.custid = m.custid AND crm.tenant_code = @tenant LIMIT 1),
                               (SELECT crm.custcode FROM customerdb.customer_registration_master crm WHERE crm.custid = m.custid ORDER BY crm.regid DESC LIMIT 1)
                           ) AS uhid
                    FROM lab_request_master m
                    WHERE {whereClause}";

                    var rows = await conn.QueryAsync(sql, new
                    {
                        tenant = keyTenant,
                        guids = guidsToQuery.ToArray(),
                        billNos = billNosToQuery.ToArray()
                    });

                    var guidMap = new Dictionary<string, (decimal? custid, string uhid)>(StringComparer.OrdinalIgnoreCase);
                    var billNoMap = new Dictionary<string, (decimal? custid, string uhid)>(StringComparer.OrdinalIgnoreCase);

                    foreach (var row in rows)
                    {
                        string rGuid = row.requestguid?.ToString() ?? "";
                        string bNo = row.requestsnoprint?.ToString() ?? "";
                        decimal? cid = row.custid != null ? (decimal)row.custid : null;
                        string? uhid = row.uhid?.ToString();
                        var clean = CleanUhid(uhid);

                        if (!string.IsNullOrWhiteSpace(clean))
                        {
                            if (!string.IsNullOrEmpty(rGuid)) guidMap[rGuid] = (cid, clean);
                            if (!string.IsNullOrEmpty(bNo)) billNoMap[bNo] = (cid, clean);

                            if (cid.HasValue && cid.Value > 0)
                            {
                                _uhidCache[(keyTenant, cid.Value)] = clean;
                                _uhidCache[("", cid.Value)] = clean;
                            }
                        }
                    }

                    foreach (var b in billList)
                    {
                        (decimal? custid, string uhid) matched = default;
                        bool found = false;

                        if (!string.IsNullOrWhiteSpace(b.requestguid) && guidMap.TryGetValue(b.requestguid, out matched))
                        {
                            found = true;
                        }
                        else if (!string.IsNullOrWhiteSpace(b.bill_no) && billNoMap.TryGetValue(b.bill_no.Trim(), out matched))
                        {
                            found = true;
                        }

                        if (found)
                        {
                            if ((!b.custid.HasValue || b.custid <= 0) && matched.custid.HasValue)
                            {
                                b.custid = matched.custid;
                            }
                            if (!string.IsNullOrWhiteSpace(matched.uhid))
                            {
                                b.custcode = matched.uhid;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PatientUhidService] Notice enriching bills: {ex.Message}");
                }
                finally
                {
                    _dbSemaphore.Release();
                }
            }
        }
    }
}
