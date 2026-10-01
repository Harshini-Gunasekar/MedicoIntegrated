using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("uom_master")]
    public class UomMasterModel
    {
        private decimal _ucode;
        [Key]
        public decimal ucode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal uomcode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal uom_code { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal unitcode { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal unit_code { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal uomid { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal uom_id { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }
        public decimal id { get => _ucode; set { if (value > 0 || _ucode == 0) _ucode = value; } }

        public string? tenant_code { get; set; }
        public string? tenantcode { get => tenant_code; set => tenant_code = value; }

        public int orderno { get; set; }
        public int? order_no { get => orderno; set => orderno = value ?? 0; }

        private string? _name;
        [Required(ErrorMessage = "Unit Name is required")]
        public string? name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? uomname { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? uom_name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? unitname { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }
        public string? unit_name { get => _name; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_name)) _name = value; } }

        private string? _shortname;
        public string? shortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? short_name { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uomshortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uom_shortname { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? unit { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }
        public string? uom { get => _shortname; set { if (!string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(_shortname)) _shortname = value; } }

        public int? decimalplaces { get; set; }

        public string? description { get; set; }

        public bool deleted { get; set; } = false;
        public bool isdeleted { get => deleted; set => deleted = value; }
        public bool is_deleted { get => deleted; set => deleted = value; }

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;

        public int? packsize { get; set; }
    }
}
