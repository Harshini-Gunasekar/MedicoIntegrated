using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Booking.Helpers
{
    public static class ExcelDateNormalizer
    {
        // Strictly valid calendar month names and standard abbreviations (NO partial substring matching)
        private static readonly Dictionary<string, int> ExactMonthMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["jan"] = 1, ["january"] = 1,
            ["feb"] = 2, ["february"] = 2,
            ["mar"] = 3, ["march"] = 3,
            ["apr"] = 4, ["april"] = 4,
            ["may"] = 5,
            ["jun"] = 6, ["june"] = 6,
            ["jul"] = 7, ["july"] = 7,
            ["aug"] = 8, ["august"] = 8,
            ["sep"] = 9, ["sept"] = 9, ["september"] = 9,
            ["oct"] = 10, ["october"] = 10,
            ["nov"] = 11, ["november"] = 11,
            ["dec"] = 12, ["december"] = 12
        };

        // Regex matching dates like 01-May-27, 30-Apr-29, 01/May/2027
        private static readonly Regex DayMonthYearRegex = new(
            @"^(\d{1,2})[-/. ]([A-Za-z]+)[-/. ](\d{2,4})$",
            RegexOptions.Compiled);

        // Regex matching dates like May-01-27, May-1-2027
        private static readonly Regex MonthDayYearRegex = new(
            @"^([A-Za-z]+)[-/. ](\d{1,2})[-/. ](\d{2,4})$",
            RegexOptions.Compiled);

        // Regex matching dates like May-27, Apr-29, May-2027 (strictly exact month name, e.g. NOT Febutax-40)
        private static readonly Regex MonthYearRegex = new(
            @"^([A-Za-z]+)[-/. ](\d{2,4})$",
            RegexOptions.Compiled);

        // Regex matching numeric dates like 01-05-27, 01/05/2027
        private static readonly Regex NumericDayMonthYearRegex = new(
            @"^(\d{1,2})[-/.](\d{1,2})[-/.](\d{2,4})$",
            RegexOptions.Compiled);

        // Regex matching numeric month-year like 05/27, 04/29, 05/2027
        private static readonly Regex NumericMonthYearRegex = new(
            @"^(\d{1,2})[-/.](\d{2,4})$",
            RegexOptions.Compiled);

        // XML tag match for text cells: <t>...</t> or <t xml:space="...">...</t>
        private static readonly Regex TagTextRegex = new(
            @"<t(?:\s+[^>]*)?>([^<]+)</t>",
            RegexOptions.Compiled);

        // XML tag match for value cells: <v>...</v>
        private static readonly Regex TagValueRegex = new(
            @"<v>([^<]+)</v>",
            RegexOptions.Compiled);

        public static string? TryNormalizeDateString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var val = input.Trim();

            // Placeholders mean no expiry date
            if (val == "-" || val.Equals("NA", StringComparison.OrdinalIgnoreCase) ||
                val.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                val.Equals("NIL", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Already yyyy-MM-dd
            if (Regex.IsMatch(val, @"^\d{4}-\d{2}-\d{2}$"))
                return val;

            // 1. Day-Month-Year (e.g. 01-May-27, 01-Feb-28, 30-Apr-29)
            var m1 = DayMonthYearRegex.Match(val);
            if (m1.Success)
            {
                string monWord = m1.Groups[2].Value;
                if (ExactMonthMap.TryGetValue(monWord, out int mon) && mon >= 1 && mon <= 12)
                {
                    int day = int.Parse(m1.Groups[1].Value);
                    int year = int.Parse(m1.Groups[3].Value);
                    if (year < 100) year += 2000;

                    int maxDays = DateTime.DaysInMonth(year, mon);
                    day = Math.Clamp(day, 1, maxDays);

                    return $"{year:D4}-{mon:D2}-{day:D2}";
                }
            }

            // 2. Month-Day-Year (e.g. May-01-27, Feb-01-28)
            var mMonthDay = MonthDayYearRegex.Match(val);
            if (mMonthDay.Success)
            {
                string monWord = mMonthDay.Groups[1].Value;
                if (ExactMonthMap.TryGetValue(monWord, out int mon) && mon >= 1 && mon <= 12)
                {
                    int day = int.Parse(mMonthDay.Groups[2].Value);
                    int year = int.Parse(mMonthDay.Groups[3].Value);
                    if (year < 100) year += 2000;

                    int maxDays = DateTime.DaysInMonth(year, mon);
                    day = Math.Clamp(day, 1, maxDays);

                    return $"{year:D4}-{mon:D2}-{day:D2}";
                }
            }

            // 3. Month-Year (e.g. May-27, Feb-28) -> Pass 1st of month (e.g. 2028-02-01)
            // MUST be an exact calendar month name (prevents item names like 'FEBUTAX 40' from ever matching)
            var m2 = MonthYearRegex.Match(val);
            if (m2.Success)
            {
                string monWord = m2.Groups[1].Value;
                if (ExactMonthMap.TryGetValue(monWord, out int mon) && mon >= 1 && mon <= 12)
                {
                    int year = int.Parse(m2.Groups[2].Value);
                    if (year < 100) year += 2000;

                    return $"{year:D4}-{mon:D2}-01";
                }
            }

            // 4. Numeric Day-Month-Year (e.g. 01-05-2027, 01/02/28)
            var m3 = NumericDayMonthYearRegex.Match(val);
            if (m3.Success)
            {
                int p1 = int.Parse(m3.Groups[1].Value);
                int p2 = int.Parse(m3.Groups[2].Value);
                int year = int.Parse(m3.Groups[3].Value);
                if (year < 100) year += 2000;

                int day = p1;
                int mon = p2;
                if (mon >= 1 && mon <= 12 && day >= 1 && day <= 31)
                {
                    int maxDays = DateTime.DaysInMonth(year, mon);
                    day = Math.Clamp(day, 1, maxDays);

                    return $"{year:D4}-{mon:D2}-{day:D2}";
                }
            }

            // 5. Numeric Month-Year (e.g. 02/28, 05/27) -> Pass 1st of month (e.g. 2028-02-01)
            var m4 = NumericMonthYearRegex.Match(val);
            if (m4.Success)
            {
                int mon = int.Parse(m4.Groups[1].Value);
                int year = int.Parse(m4.Groups[2].Value);
                if (year < 100) year += 2000;

                if (mon >= 1 && mon <= 12)
                {
                    return $"{year:D4}-{mon:D2}-01";
                }
            }

            return null;
        }

        public static async Task<byte[]> NormalizeExcelDatesAsync(Stream inputStream)
        {
            var ms = new MemoryStream();
            await inputStream.CopyToAsync(ms);
            ms.Position = 0;

            try
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Update, leaveOpen: true))
                {
                    var utf8NoBom = new UTF8Encoding(false);

                    // 1. Read sharedStrings.xml if present
                    var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
                    string? sharedStringsXml = null;
                    var sharedStringsList = new List<string>();

                    if (sharedStringsEntry != null)
                    {
                        using (var reader = new StreamReader(sharedStringsEntry.Open(), Encoding.UTF8))
                        {
                            sharedStringsXml = await reader.ReadToEndAsync();
                        }

                        var siMatches = Regex.Matches(sharedStringsXml, @"<si>(.*?)</si>", RegexOptions.Singleline);
                        foreach (Match si in siMatches)
                        {
                            var tm = TagTextRegex.Match(si.Value);
                            sharedStringsList.Add(tm.Success ? tm.Groups[1].Value : "");
                        }
                    }

                    // 2. Scan sheet(s) to detect Expiry Date column(s)
                    var sheetEntries = new List<string>();
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase) &&
                            entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            sheetEntries.Add(entry.FullName);
                        }
                    }

                    var sharedIndicesToNormalize = new HashSet<int>();
                    bool anyExpiryColumnFound = false;

                    foreach (var sheetName in sheetEntries)
                    {
                        var entry = archive.GetEntry(sheetName);
                        if (entry == null) continue;

                        string sheetXml;
                        using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                        {
                            sheetXml = await reader.ReadToEndAsync();
                        }

                        // Find header row (row 1 or first row) to find expiry column letters
                        var headerRowMatch = Regex.Match(sheetXml, @"<row\s+r=""(\d+)""[^>]*>(.*?)</row>", RegexOptions.Singleline);
                        var expCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        if (headerRowMatch.Success)
                        {
                            var headerCells = Regex.Matches(headerRowMatch.Groups[2].Value, @"<c\s+r=""([A-Z]+)\d+""([^>]*)>(.*?)</c>", RegexOptions.Singleline);
                            foreach (Match hc in headerCells)
                            {
                                string colLetter = hc.Groups[1].Value;
                                string attrs = hc.Groups[2].Value;
                                string inner = hc.Groups[3].Value;
                                string headerText = "";

                                var vm = Regex.Match(inner, @"<v>(\d+)</v>");
                                if (vm.Success && attrs.Contains(@"t=""s""") && int.TryParse(vm.Groups[1].Value, out int sIdx) && sIdx < sharedStringsList.Count)
                                {
                                    headerText = sharedStringsList[sIdx];
                                }
                                else
                                {
                                    var tm = TagTextRegex.Match(inner);
                                    if (tm.Success) headerText = tm.Groups[1].Value;
                                }

                                var cleanHeader = Regex.Replace(headerText.ToLowerInvariant(), @"[\s_]", "");
                                if (cleanHeader.Contains("exp") || cleanHeader.Contains("expiry"))
                                {
                                    expCols.Add(colLetter);
                                }
                            }
                        }

                        if (expCols.Count > 0)
                        {
                            anyExpiryColumnFound = true;
                        }

                        bool sheetModified = false;
                        string updatedSheetXml = Regex.Replace(sheetXml, @"<c\s+r=""([A-Z]+)(\d+)""([^>]*)>(.*?)</c>", m =>
                        {
                            string colLetter = m.Groups[1].Value;
                            int rowNum = int.Parse(m.Groups[2].Value);
                            string attrs = m.Groups[3].Value;
                            string inner = m.Groups[4].Value;

                            // Skip header row
                            if (headerRowMatch.Success && rowNum == int.Parse(headerRowMatch.Groups[1].Value))
                                return m.Value;

                            // If we identified expiry columns, ONLY process cells in those columns!
                            // Column A (Item Name) and Column B (Batch No) are strictly protected.
                            if (expCols.Count > 0 && !expCols.Contains(colLetter))
                                return m.Value;

                            // Check if cell uses shared strings: <v>index</v>
                            var vm = Regex.Match(inner, @"<v>(\d+)</v>");
                            if (vm.Success && attrs.Contains(@"t=""s""") && int.TryParse(vm.Groups[1].Value, out int idx))
                            {
                                if (idx < sharedStringsList.Count)
                                {
                                    string raw = sharedStringsList[idx];
                                    string? norm = TryNormalizeDateString(raw);
                                    if (norm != null && norm != raw)
                                    {
                                        sharedIndicesToNormalize.Add(idx);
                                    }
                                }
                                return m.Value;
                            }

                            // Inline strings <is><t>...</t></is>
                            if (inner.Contains("<t"))
                            {
                                string updatedInner = TagTextRegex.Replace(inner, tm =>
                                {
                                    string raw = tm.Groups[1].Value;
                                    string? norm = TryNormalizeDateString(raw);
                                    if (norm != null && norm != raw)
                                    {
                                        sheetModified = true;
                                        return $"<t>{norm}</t>";
                                    }
                                    return tm.Value;
                                });
                                if (sheetModified)
                                {
                                    return $"<c r=\"{colLetter}{rowNum}\"{attrs}>{updatedInner}</c>";
                                }
                            }

                            // Direct string values <v>...</v>
                            if (attrs.Contains(@"t=""str"""))
                            {
                                string updatedInner = TagValueRegex.Replace(inner, vm2 =>
                                {
                                    string raw = vm2.Groups[1].Value;
                                    string? norm = TryNormalizeDateString(raw);
                                    if (norm != null && norm != raw)
                                    {
                                        sheetModified = true;
                                        return $"<v>{norm}</v>";
                                    }
                                    return vm2.Value;
                                });
                                if (sheetModified)
                                {
                                    return $"<c r=\"{colLetter}{rowNum}\"{attrs}>{updatedInner}</c>";
                                }
                            }

                            return m.Value;
                        }, RegexOptions.Singleline);

                        if (sheetModified)
                        {
                            entry.Delete();
                            var newEntry = archive.CreateEntry(sheetName);
                            using var writer = new StreamWriter(newEntry.Open(), utf8NoBom);
                            await writer.WriteAsync(updatedSheetXml);
                            await writer.FlushAsync();
                        }
                    }

                    // 3. Update sharedStrings.xml (ONLY indices identified as expiry dates)
                    if (sharedStringsEntry != null && sharedStringsXml != null)
                    {
                        bool ssModified = false;
                        int currentIndex = 0;

                        string updatedSharedStringsXml = Regex.Replace(sharedStringsXml, @"<si>(.*?)</si>", m =>
                        {
                            int idx = currentIndex++;
                            string siContent = m.Groups[1].Value;

                            // If we detected expiry columns, ONLY update indices referenced by expiry columns!
                            // If no expiry columns were detected, only update if it strictly matches a valid date.
                            bool shouldNormalize = anyExpiryColumnFound
                                ? sharedIndicesToNormalize.Contains(idx)
                                : true;

                            if (shouldNormalize)
                            {
                                string updatedSi = TagTextRegex.Replace(siContent, tm =>
                                {
                                    string raw = tm.Groups[1].Value;
                                    string? norm = TryNormalizeDateString(raw);
                                    if (norm != null && norm != raw)
                                    {
                                        ssModified = true;
                                        return $"<t>{norm}</t>";
                                    }
                                    return tm.Value;
                                });
                                return $"<si>{updatedSi}</si>";
                            }

                            return m.Value;
                        }, RegexOptions.Singleline);

                        if (ssModified)
                        {
                            sharedStringsEntry.Delete();
                            var newEntry = archive.CreateEntry("xl/sharedStrings.xml");
                            using var writer = new StreamWriter(newEntry.Open(), utf8NoBom);
                            await writer.WriteAsync(updatedSharedStringsXml);
                            await writer.FlushAsync();
                        }
                    }
                }

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExcelDateNormalizer] Warning: {ex.Message}");
                return ms.ToArray();
            }
        }
    }
}
