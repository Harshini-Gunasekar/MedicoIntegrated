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
        private long _ucode;
        [Key]
        public long ucode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long uomcode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long uom_code { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long unitcode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long unit_code { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long uomid { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long uom_id { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public long id { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }

        public int orderno { get; set; }
        public int? order_no { get => orderno; set => orderno = value ?? 0; }

        private string? _name;
        public string? name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? uomname { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? uom_name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? unitname { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? unit_name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }

        private string? _shortname;
        public string? shortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? short_name { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uomshortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uom_shortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? unit { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uom { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }

        public string? description { get; set; }
        public bool? deleted { get; set; } = false;
        public bool? isdeleted { get => deleted; set => deleted = value; }
        public bool? is_deleted { get => deleted; set => deleted = value; }

        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public int? packsize { get; set; }
        public int? decimalplaces { get; set; }
        public string? tenant_code { get; set; }
        public string? tenantcode { get => tenant_code; set => tenant_code = value; }
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
        public bool isdoctor_room { get; set; } = false;
        public bool isdoctorroom { get => isdoctor_room; set => isdoctor_room = value; }
        public bool isdoctorallowed { get => isdoctor_room; set => isdoctor_room = value; }
        public bool isdoctor_allowed { get => isdoctor_room; set => isdoctor_room = value; }
        public bool is_doctor_room { get => isdoctor_room; set => isdoctor_room = value; }
        public bool doctorroom { get => isdoctor_room; set => isdoctor_room = value; }
        public string? druglicenseno { get; set; }
        public string? pharmacyregistrationno { get; set; }
        public string? gstno { get; set; }
        public string? msmeno { get; set; }
    }

    public class item_master
    {
        private int _itemcode;
        public int itemcode { get => _itemcode; set { if (value > 0 || _itemcode == 0) _itemcode = value; } }
        public int item_code { get => _itemcode; set { if (value > 0 || _itemcode == 0) _itemcode = value; } }
        public int drugcode { get => _itemcode; set { if (value > 0 || _itemcode == 0) _itemcode = value; } }
        public int drug_code { get => _itemcode; set { if (value > 0 || _itemcode == 0) _itemcode = value; } }
        public int id { get => _itemcode; set { if (value > 0 || _itemcode == 0) _itemcode = value; } }

        private string? _itemname;
        public string? itemname { get => _itemname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_itemname)) _itemname = value; } }
        public string? item_name { get => _itemname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_itemname)) _itemname = value; } }

        public string? shortname { get; set; }
        public string? short_name { get => shortname; set => shortname = value; }
        public string? description { get; set; }
        public int categorycode { get; set; }
        public int subcategorycode { get; set; }
        public int hsnCode { get; set; }
        public string? itemtype { get; set; }
        public decimal gstpercentage { get; set; }

        private int _uomcode;
        public int uomcode { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int uom_code { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int ucode { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int unitcode { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int unit_code { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int uomid { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }
        public int uom_id { get => _uomcode; set { if (value > 0 || _uomcode == 0) _uomcode = value; } }

        public string? uom { get; set; }
        public string? unit { get; set; }
        public string? uomname { get; set; }
        public string? uom_name { get; set; }
        public string? uomshortname { get; set; }
        public string? uom_shortname { get; set; }

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

        private string? _drugname;
        public string? drugname { get => _drugname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_drugname)) _drugname = value; } }
        public string? drug_name { get => _drugname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_drugname)) _drugname = value; } }
        public string? medicine_name { get => _drugname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_drugname)) _drugname = value; } }

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

