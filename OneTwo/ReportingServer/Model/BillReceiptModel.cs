namespace ReportingServer.Model
{
    public class BillReceiptModel
    {
    }
    public class BillModel
    {
        public string TestName { get; set; }

        public decimal Amount { get; set; }
    }
    public class BillReceiptData
    {
        public string CustomerName { get; set; }

        public string InvoiceNo { get; set; }

        public decimal BillAmount { get; set; }

        public DateTime BillDate { get; set; }
    }

    public class CashBillModel
    {

        public string TenantId { get; set; }
        public byte[]? logo { get; set; }
        public byte[]? headerimage { get; set; }
        public byte[]? footerimage { get; set; }

        public bool isletterhead { get; set; } = false;
        // Header
        public string LabName { get; set; }
        public string? BranchName { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string HelplineNo { get; set; }

        // Bill Info
        public string BillNo { get; set; }
        public string? RequestSnoPrint { get; set; }
        public DateTime BillDate { get; set; }

        // Patient Info
        public string PatientName { get; set; }
        public string? PatientId { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string? PatientAddress { get; set; }
        public string? CusMobileNo { get; set; }
        public string? CareOf { get; set; }
        public string DoctorName { get; set; }

        // Online Info
        public string? OnlineCode { get; set; }
        public string? Password { get; set; }

        // Footer
        public string? CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        // Test Details
        public IList<CashBillItemModel> Items { get; set; } = new List<CashBillItemModel>();
    }

    public class CashBillItemModel
    {
        public int SNo { get; set; }

        public string TestName { get; set; }

        public decimal Amount { get; set; }
    }

    public static class AmountToWordsConverter
    {
        public static string Convert(decimal amount)
        {
            try
            {
                if (amount == 0)
                    return "Zero Rupees Only";

                long val = (long)Math.Floor(amount);
                string words = NumberToWords(val);
                
                // Convert to Title Case (e.g., "One Hundred And Twenty")
                words = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words.ToLower());
                
                return words + " Rupees Only";
            }
            catch
            {
                return "Zero Rupees Only";
            }
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }
    }

    public class BillingRequest
    {
        public CashBillModel? BillData { get; set; }
        public byte[]? LogoImage { get; set; }
    }
}
