using System;
using Dapper.Contrib.Extensions;

namespace LIMS_Backend.Model
{
    [Table("cash_voucher")]
    public class CashVoucher
    {
        [ExplicitKey]
        public Guid cashvoucherid { get; set; }

        public int? voucherno { get; set; }
        public string? voucherbarcode { get; set; }
        public string? voucherconvertedbarcode { get; set; }

        public DateTimeOffset? expensedate { get; set; }

        public long? ledgerid { get; set; }
        public long? cashledgerid { get; set; }

        public int? bhcode { get; set; }

        public string? paidto { get; set; }
        public string? description { get; set; }

        public decimal? amount { get; set; }

        public bool? deleted { get; set; } = false;

        public int? usercode { get; set; }
        public int? computercode { get; set; }

        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }

        public decimal? ctcode { get; set; }
        public int? tmcode { get; set; }

        public decimal? cntcode { get; set; }
        public Guid? cnttid { get; set; }

        public bool? ispayment { get; set; }
        public bool? isreceipt { get; set; }
        public bool? isposted { get; set; }

        public string? tenant_code { get; set; }
    }
}
