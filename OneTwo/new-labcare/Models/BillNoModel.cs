using Dapper.Contrib.Extensions;


namespace LIMS_Backend.Model
{
    [Table("billno_master")]
    public class BillNoModel
    {
        [Key]
        public long bncode { get; set; }


        public int orderno { get; set; }


        public string? name { get; set; }


        public string? shortname { get; set; }


        public string? description { get; set; }


        public bool? allbranch { get; set; }


        public bool? allcounter { get; set; }


        public int?  bhcode { get; set; }
        public long? cntcode { get; set; }


        public bool? restartfinancialyear { get; set; }


        public bool? restartcalendaryear { get; set; }


        public bool? restartdaily { get; set; }


        public bool? issampleno { get; set; } = true;
        public bool? isreceiptno { get; set; } = true;


        public bool? deleted { get; set; } = false;


        public int? usercode { get; set; } = 1;


        public int? computercode { get; set; } = 1;


        public DateTime? entereddate { get; set; }


        public DateTime? ibsdate { get; set; }
        public bool? restartmonthly { get; set; }
        public string? tenant_code { get; set; }
    }
  


    [Table("billtime")]
    public class BillTimeModel
    {
        [Key]
        public long tmcode { get; set; }
        public decimal? bhcode { get; set; }
        public DateTime? billdate { get; set; }   // date only (midnight)
        public DateTime? fromdate { get; set; }   // billing window start
        public DateTime? todate { get; set; }   // billing window end
    }

    [Table("billno_mapping")]
    public class BillNoMappingModel
    {
        [Key]
        public long map_id { get; set; }
        public long bncode { get; set; }
        public int? bhcode { get; set; }
        public long? cntcode { get; set; }
        public int? usercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public bool? deleted { get; set; } = false;
        public string? tenant_code { get; set; }
    }

    public class BillNoMappingItemDto
    {
        public long? map_id { get; set; }       
        public int? bhcode { get; set; }
        public long? cntcode { get; set; }
    }

    public class BillNoConfigurationDto
    {
        public long? bncode { get; set; }        
        public int orderno { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public bool? allbranch { get; set; }
        public bool? allcounter { get; set; }
        public bool? restartfinancialyear { get; set; }
        public bool? restartcalendaryear { get; set; }
        public bool? restartdaily { get; set; }
        public bool? issampleno { get; set; } = true;
        public bool? isreceiptno { get; set; } = true;
        public bool? restartmonthly { get; set; }
        public bool? deleted { get; set; } // Added manually just in case filtering relies on it
        public List<BillNoMappingItemDto> mappings { get; set; } = new();
    }
}
