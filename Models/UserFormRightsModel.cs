using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace LIMS_Backend.Model
{
    public class UserFormRightsModel
    {
        // ─── Table Models ─────────────────────────────────────────────────────

        [Table("user_form_rights")]
        public class user_form_rights
        {
            [Key]
            public int dcode { get; set; }
            public int? ibsdcode { get; set; }
            public int orderno { get; set; }
            public int rcode { get; set; }
            public int rusercode { get; set; }
            public string? mnuname { get; set; }
            public string? mnucaption { get; set; }
            public bool deleted { get; set; }
            public int usercode { get; set; }
            public int computercode { get; set; }
            public DateTime entereddate { get; set; }
            public DateTime ibsdate { get; set; }
            public string? tenant_code { get; set; }
        }

        [Table("userauthorization")]
        public class userauthorization
        {
            [ExplicitKey]
            public Guid uasid { get; set; }
            public int gcode { get; set; }
            public int usercode { get; set; }
            public string? tenant_code { get; set; }
        }
    }
}

