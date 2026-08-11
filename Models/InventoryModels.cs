using System;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
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

    public class vendor_master
    {
        [Key]
        public int vendorCode { get; set; }
        public string? vendorName { get; set; }
        public string? shortName { get; set; }
        public string? vendorType { get; set; }
        public string? contactPerson { get; set; }
        public string? phoneNumber { get; set; }
        public string? alternatePhoneNumber { get; set; }
        public string? emailId { get; set; }
        public string? website { get; set; }
        public string? gstNumber { get; set; }
        public string? panNumber { get; set; }
        public string? taxId { get; set; }
        public string? registrationNumber { get; set; }
        public string? addressLine1 { get; set; }
        public string? addressLine2 { get; set; }
        public string? landmark { get; set; }
        public string? city { get; set; }
        public string? district { get; set; }
        public string? state { get; set; }
        public string? postalCode { get; set; }
        public string? countryCode { get; set; }
        public string? countryName { get; set; }
        public string? currencyCode { get; set; }
        public string? paymentTerms { get; set; }
        public string? creditPeriod { get; set; }
        public string? bankName { get; set; }
        public string? accountNumber { get; set; }
        public string? ifscCode { get; set; }
        public string? swiftCode { get; set; }
        public string? ibanNumber { get; set; }
        public bool isActive { get; set; } = true;
        public bool deleted { get; set; } = false;
        public int userCode { get; set; }
        public string? tenantCode { get; set; }
        public string? branchCode { get; set; }
        public DateTime createddate { get; set; } = DateTime.Now;
        public string? druglicenseno { get; set; }
        public string? fssaino { get; set; }
        public decimal? vendorrating { get; set; }
    }

    public class manufacturer_master
    {
        public long manufacturercode { get; set; }
        public string manufacturername { get; set; }
        public string shortname { get; set; }
        public string description { get; set; }
        public string contactperson { get; set; }
        public string phoneno { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string gstno { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public DateTime createddate { get; set; }
        public int usercode { get; set; }
        public string tenantcode { get; set; }
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

    public class ledger_group_master
    {
        public int ledgergroupcode { get; set; }
        public string ledgergroupname { get; set; }
        public string shortname { get; set; }
        public int ledgertypecode { get; set; }
        public string description { get; set; }
        public bool isactive { get; set; } = true;
        public DateTime createddate { get; set; } = DateTime.Now;
        public string tenantcode { get; set; }
        public bool deleted { get; set; } = false;
    }

    public class ledger_type_master
    {
        public int ledgertypecode { get; set; }
        public string ledgertypename { get; set; }
        public string shortname { get; set; }
        public string description { get; set; }
        public int naturetype { get; set; }
        public bool isactive { get; set; } = true;
        public DateTime createddate { get; set; } = DateTime.Now;
        public string tenantcode { get; set; }
        public bool isgstapplicable { get; set; } = false;
        public bool isvatapplicable { get; set; } = false;
        public decimal sgstpercentage { get; set; } = 0;
        public decimal cgstpercentage { get; set; } = 0;
        public decimal igstpercentage { get; set; } = 0;
        public bool deleted { get; set; } = false;
    }

    public class warehouse_master
    {
        public int? warehousecode { get; set; }
        public int orderno { get; set; }
        public string warehousename { get; set; }
        public string shortname { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string? tenantcode { get; set; }
        public bool isactive { get; set; }
        public bool isdeleted { get; set; }
        public DateTime createddate { get; set; }
        public bool purchaseallow { get; set; } = false;
        public bool salesallow { get; set; } = false;
    }

    public class item_master
    {
        public int itemcode { get; set; }
        public string? itemname { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public int categorycode { get; set; }
        public int subcategorycode { get; set; }
        public int hsnCode { get; set; }
        public string? itemtype { get; set; }
        public decimal gstpercentage { get; set; }
        public int uomcode { get; set; }
        public decimal purchaserate { get; set; }
        public decimal salesrate { get; set; }
        public decimal mrp { get; set; }
        public decimal currentstock { get; set; }
        public decimal minstock { get; set; }
        public decimal reorderlevel { get; set; }
        public decimal packsize { get; set; }
        public bool isexpiry { get; set; }
        public int expiryalertdays { get; set; }
        public bool expiryrequired { get; set; }
        public bool serialrequired { get; set; }
        public int brandcode { get; set; }
        public int manufacturercode { get; set; }
        public int taxcode { get; set; }
        public int naturetype { get; set; }
        public string? manufacturername { get; set; }
        public string? manufacturer { get; set; }
        public int ledgergroupcode { get; set; }
        public string? drugname { get; set; }
        public string? packaging { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public DateTime createddate { get; set; }
        public int usercode { get; set; }
        public string? tenantcode { get; set; }
        public string? schedule { get; set; }
        public bool isnarcoticdrug { get; set; } = false;
    }

    public class inventory_item_master : item_master
    {
    }
}

