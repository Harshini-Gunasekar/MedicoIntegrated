using System;

namespace Booking.Models
{
    public class OpChargeSlabModel
    {
        public int slabid { get; set; }

        public string? tenant_code { get; set; }

        public int dcode { get; set; }

        public int min_age { get; set; }

        public int max_age { get; set; }

        public double opcharge { get; set; }

        public bool deleted { get; set; } = false;

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }
}
