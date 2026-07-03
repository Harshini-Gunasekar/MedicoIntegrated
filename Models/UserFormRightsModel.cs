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

        [Table("mastertenant.usermodulerights")]
        public class usermodulerights
        {
            [Key]
            public int usermodulesrightsid { get; set; }
            public int usercode { get; set; }
            public int usermoduleid { get; set; }
            public bool toadd { get; set; }
            public bool toview { get; set; }
            public bool toedit { get; set; }
            public bool todelete { get; set; }
            public string? tenant_code { get; set; }
        }

        [Table("mastertenant.usermodules")]
        public class usermodules
        {
            [Key]
            public int usermoduleid { get; set; }
            public int sno { get; set; }
            public string? modulename { get; set; }
            public string? department { get; set; }
            public string? tenant_code { get; set; }
        }

        public class User_Rights
        {
            public int UserModuleID { get; set; }
            public int Sno { get; set; }
            public string? ModuleName { get; set; }
            public string? Department { get; set; }
            public int UserModuleRightsID { get; set; }
            public int UserCode { get; set; }
            public bool ToAdd { get; set; }
            public bool ToView { get; set; }
            public bool ToEdit { get; set; }
            public bool ToDelete { get; set; }
        }

        public class App_User_Rights : User_Rights { }

        public class ModuleRightsUpsertRequest
        {
            public int usercode { get; set; }
            public IList<usermodulerights> rights { get; set; } = new List<usermodulerights>();
        }
    }
}
