using System;

namespace Inventory_management_frontend.Models
{
    public class ledger_type_master
    {
        public int ledgertypecode { get; set; }
        public string ledgertypename { get; set; }
        public string shortname { get; set; }
        public string description { get; set; }

        // Nature Type
        // 1 = Asset
        // 2 = Liability
        // 3 = Expense
        // 4 = Income
        public int naturetype { get; set; }

        public bool isactive { get; set; } = true;

        public DateTime createddate { get; set; } = DateTime.Now;

        public string tenantcode { get; set; }

        // GST Details
        public bool isgstapplicable { get; set; } = false;

        public bool isvatapplicable { get; set; } = false;

        public decimal sgstpercentage { get; set; } = 0;

        public decimal cgstpercentage { get; set; } = 0;

        public decimal igstpercentage { get; set; } = 0;

        public bool deleted { get; set; } = false;
    }

}
