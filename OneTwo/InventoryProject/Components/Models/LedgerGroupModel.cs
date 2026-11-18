using System;

namespace Inventory_management_frontend.Models
{
    public class ledger_group_master
    {
        public int ledgergroupcode { get; set; }

        public string ledgergroupname { get; set; }

        public string shortname { get; set; }

        // FK -> LedgerTypeMaster
        public int ledgertypecode { get; set; }

        public string description { get; set; }

        public bool isactive { get; set; } = true;

        public DateTime createddate { get; set; } = DateTime.Now;

        public string tenantcode { get; set; }

        public bool deleted { get; set; } = false;

    }
}
