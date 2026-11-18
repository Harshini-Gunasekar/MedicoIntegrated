namespace SharedComponents.Rcl.Models
{
    /// <summary>
    /// Lightweight DTO representing a user's module rights.
    /// Mirrors LIMS_Backend.Model.UserFormRightsModel.User_Rights without Dapper dependency.
    /// </summary>
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
        public bool ToExport { get; set; }
    }
}
