using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class indent_master
    {
        public long indentcode { get; set; }

        // Indent Info
        public string? indentno { get; set; }
        public DateTime indentdate { get; set; }

        // Request Info
        public long? requestedby { get; set; }
        public long? departmentcode { get; set; }
        public string? branchcode { get; set; }
        public long purchasedetailcode { get; set; }

        public decimal issuedqty { get; set; }
        // Remarks
        public string? remarks { get; set; }

        // Approval
        public string? approvalstatus { get; set; }   // PENDING / APPROVED / REJECTED
        public long? approvedby { get; set; }
        public DateTime? approveddate { get; set; }
        public string? approvalremarks { get; set; }

        // Status
        public bool isactive { get; set; }
        public bool deleted { get; set; }

        // Audit
        public DateTime createddate { get; set; }

        // Multi Tenant
        public string? tenantcode { get; set; }
    }

    public class indent_detail
    {
        public long indentdetailcode { get; set; }

        // Parent Indent
        public long indentcode { get; set; }

        // Item Reference
        public long itemcode { get; set; }
        public long purchasedetailcode { get; set; }

        // Quantity
        public decimal requestedqty { get; set; }
        public decimal approvedqty { get; set; }
        public decimal issuedqty { get; set; }

        // Remarks
        public string? remarks { get; set; }
    }

    public class indent_request
    {
        public indent_master master { get; set; }
        public List<indent_detail> details { get; set; }
    }
}