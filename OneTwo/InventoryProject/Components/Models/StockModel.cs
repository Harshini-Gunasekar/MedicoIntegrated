using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_management_frontend.Models
{
    public class stock_master
    {
        [Key]
        public long stockcode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Item selection is required")]
        public long itemcode { get; set; }

        public long? warehousecode { get; set; }
        public string? branchcode { get; set; }
        public string? locationcode { get; set; }

        public decimal openingstock { get; set; }
        public decimal purchasedqty { get; set; }
        public decimal soldqty { get; set; }
        public decimal damagedqty { get; set; }
        public decimal returnqty { get; set; }
        public decimal closingstock { get; set; }

        public decimal unitcost { get; set; }
        public decimal stockvalue { get; set; }

        public string? batchno { get; set; }
        public DateTime? manufacturingdate { get; set; }
        public DateTime? expirydate { get; set; }

        public bool isactive { get; set; } = true;
        public bool deleted { get; set; } = false;

        public DateTime createddate { get; set; } = DateTime.Now;
        public DateTime? modifieddate { get; set; }
        public long? usercode { get; set; }

        public string? tenantcode { get; set; }
        public string? companycode { get; set; }
    }
}
