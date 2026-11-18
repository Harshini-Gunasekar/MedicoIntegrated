using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_management_frontend.Models
{
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
}
