using Dapper.Contrib.Extensions;
using System;

namespace Medico_Backend.Model
{
    [Table("counter_timing")]
    public class CounterTimingModel
    {
        [ExplicitKey]
        public string cnttid { get; set; } = string.Empty;

        public int? bhcode { get; set; }

        public int? cntcode { get; set; }

        public int shiftsno { get; set; }

        public DateTime? counterdate { get; set; }

        public DateTime? fromdate { get; set; }

        public DateTime? todate { get; set; }
        public string? shift_mode { get; set; }
        public DateTime? planned_to { get; set; }
        public string? close_type { get; set; }

        public string? tenant_code { get; set; }

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.UtcNow;

        public DateTime ibsdate { get; set; } = DateTime.UtcNow;
    }
}
