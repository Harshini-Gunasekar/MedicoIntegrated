using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_management_frontend.Models
{
    public class item_master
    {
    public int itemcode { get; set; }
    public string itemname { get; set; }
    public string shortname { get; set; }
    public string description { get; set; }

    public int categorycode { get; set; }
    public int subcategorycode { get; set; }

    public int hsnCode { get; set; }

    public string itemtype { get; set; }

    public decimal gstpercentage { get; set; }

    public int uomcode { get; set; }

    public decimal purchaserate { get; set; }

    public decimal salesrate { get; set; }

    public decimal mrp { get; set; }

    public decimal currentstock { get; set; }

    public decimal minstock { get; set; }

    public decimal reorderlevel { get; set; }
    public decimal packsize { get; set; }
    public bool isexpiry { get; set; } = false;

    // Alert before expiry (days)
    public int? expiryalertdays { get; set; }


    public bool batchrequired { get; set; }

    public bool expiryrequired { get; set; }

    public bool serialrequired { get; set; }

    public int brandcode { get; set; }

    public int manufacturercode { get; set; }

    public int taxcode { get; set; }

    // New Fields

    public int naturetype { get; set; }

    // Asset = 1
    // Liability = 2
    // Expense = 3
    // Income = 4

    public string? manufacturer { get; set; }
    
    public int ledgergroupcode { get; set; }
    public string? drugname { get; set; }

    public string? packaging { get; set; }

    public bool isactive { get; set; }

    public bool deleted { get; set; }

    public DateTime createddate { get; set; }

    public int usercode { get; set; }

    public string tenantcode { get; set; }
    public string schedule { get; set; }      // OTC, VH, HI, X
    public bool isnarcoticdrug { get; set; }  // true/false

 }
    

    

    public class uom_master
    {
        [Key]
        public long ucode { get; set; }
        public int orderno { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public bool? deleted { get; set; } = false;
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public int? packsize { get; set; }
        public int? decimalplaces { get; set; }
        public string? tenant_code { get; set; }
    }

    public class parent_category_master
    {
        [Key]
        public int parentcategorycode { get; set; }
        public string? parentcategoryname { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public bool isactive { get; set; } = true;
        public bool deleted { get; set; } = false;
        public DateTime createddate { get; set; } = DateTime.Now;
        public string? tenantcode { get; set; }
    }

    public class category_master
    {
        [Key]
        public long categorycode { get; set; }
        public string? categoryname { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public int parentcategorycode { get; set; }
        public bool isactive { get; set; } = true;
        public bool deleted { get; set; } = false;
        public DateTime createddate { get; set; } = DateTime.Now;
        public int usercode { get; set; }
        public string? tenantcode { get; set; }
    }
    public class ledger_master
    {
        public int ledgercode { get; set; }
        public string? ledgername { get; set; }
        public string? lcode { get; set; }
        public string? ldgcode { get; set; }
        public string? taxtype { get; set; }
        public string? taxsubtype { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal gstpercentage { get; set; }
        public string? hsncode { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public DateTime createddate { get; set; }
        public string? tenantcode { get; set; }
    }
}
