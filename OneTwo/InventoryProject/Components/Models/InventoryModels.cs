using System;
using System.Collections.Generic;

namespace Inventory_management_frontend.Models
{
    public class ItemModel
    {
        public int itemcode { get; set; }
        public string? itemname { get; set; }
        public string? shortName { get; set; }
        public string? description { get; set; }
        public int categoryCode { get; set; }
        public int subCategoryCode { get; set; }
        public string? itemType { get; set; }
        public int uomCode { get; set; }
        public decimal purchaseRate { get; set; }
        public decimal salesRate { get; set; }
        public bool isActive { get; set; }
    }

    public class VendorModel
    {
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
        public bool isActive { get; set; }
        public bool deleted { get; set; }
        public int userCode { get; set; }
        public string? tenantCode { get; set; }
        public string? branchCode { get; set; }
    }

    public class StockModel
    {
        public int itemcode { get; set; }
        public int warehousecode { get; set; }
        public string? branchcode { get; set; }
        public string? locationcode { get; set; }
        public int openingstock { get; set; }
        public int purchasedqty { get; set; }
        public int soldqty { get; set; }
        public int damagedqty { get; set; }
        public int returnqty { get; set; }
        public int closingstock { get; set; }
        public decimal unitcost { get; set; }
        public decimal stockvalue { get; set; }
        public string? batchno { get; set; }
        public string? manufacturingdate { get; set; }
        public string? expirydate { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public int usercode { get; set; }
        public string? tenantcode { get; set; }
        public string? companycode { get; set; }
    }

    public class IndentMasterModel
    {
        public int indentcode { get; set; }
        public string? indentno { get; set; }
        public DateTime indentdate { get; set; }
        public int requestedby { get; set; }
        public int departmentcode { get; set; }
        public string? branchcode { get; set; }
        public string? remarks { get; set; }
        public string? approvalstatus { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public string? tenantcode { get; set; }
    }

    public class IndentDetailModel
    {
        public int itemcode { get; set; }
        public int requestedqty { get; set; }
        public int approvedqty { get; set; }
        public int issuedqty { get; set; }
        public string? remarks { get; set; }
    }

    public class IndentRequest
    {
        public IndentMasterModel master { get; set; } = new();
        public List<IndentDetailModel> details { get; set; } = new();
    }

    public class GRNMasterModel
    {
        public string? grnno { get; set; }
        public DateTime grndate { get; set; }
        public int receivedby { get; set; }
        public string? billno { get; set; }
        public DateTime billdate { get; set; }
        public string? invoiceno { get; set; }
        public string? invoicedate { get; set; }
        public int vendorcode { get; set; }
        public int totalqty { get; set; }
        public int receivedqty { get; set; }
        public decimal grossamount { get; set; }
        public decimal discountamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal othercharges { get; set; }
        public decimal netamount { get; set; }
        public string? paymentmode { get; set; }
        public string? paymentstatus { get; set; }
        public string? remarks { get; set; }
        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public int usercode { get; set; }
        public string? tenantcode { get; set; }
        public string? branchcode { get; set; }
        public string? companycode { get; set; }
    }

    public class GRNDetailModel
    {
        public int itemcode { get; set; }
        public int orderedqty { get; set; }
        public int receivedqty { get; set; }
        public int rejectedqty { get; set; }
        public int quantity { get; set; }
        public decimal rate { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal discountamount { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal taxamount { get; set; }
        public decimal amount { get; set; }
        public decimal totalamount { get; set; }
        public string? batchno { get; set; }
        public string? manufacturingdate { get; set; }
        public string? expirydate { get; set; }
        public int warehousecode { get; set; }
        public string? tenantcode { get; set; }
    }

    public class GRNRequest
    {
        public GRNMasterModel master { get; set; } = new();
        public List<GRNDetailModel> details { get; set; } = new();
    }
}
