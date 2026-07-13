using Dapper.Contrib.Extensions;

namespace Medico_Backend.Model
{
    [Table("public.bed_master")]
    public class BedMasterModel
    {
        [Key]
        public int bedcode { get; set; }

        public int orderno { get; set; }

        public string? shortname { get; set; }

        public string? bedname { get; set; }

        public int? branchcode { get; set; }

        public int? hdcode { get; set; }

        public int? rmtcode { get; set; }

        public int? wrdcode { get; set; }

        public int flrcode { get; set; }

        public string? description { get; set; }

        public bool? deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; }

        public DateTime ibsdate { get; set; }

        public bool? islaundry { get; set; }

        public string? tenant_code { get; set; }

        // Extended status fields mapped from API responses
        [Write(false)]
        public string? bed_status { get; set; }
        [Write(false)]
        public bool? is_cleaned { get; set; }
        [Write(false)]
        public DateTime? admitted_at { get; set; }
        [Write(false)]
        public DateTime? discharged_at { get; set; }
        [Write(false)]
        public DateTime? cleaned_at { get; set; }
        [Write(false)]
        public string? cleaned_by { get; set; }
    }
}

