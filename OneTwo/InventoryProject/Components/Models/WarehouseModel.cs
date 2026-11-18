public class warehouse_master
    {
        public int warehousecode { get; set; }
        public int orderno { get; set; }

        public string warehousename { get; set; }
        public string shortname { get; set; }
        public string description { get; set; }
        public string location { get; set; }

        public int tenantcode { get; set; }

        public bool isactive { get; set; }
        public bool isdeleted { get; set; }

        public DateTime createddate { get; set; }
    }
