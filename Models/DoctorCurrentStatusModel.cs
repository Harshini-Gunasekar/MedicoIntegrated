using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace Booking.Models
{
    [Dapper.Contrib.Extensions.Table("doctor_current_status")]
    [System.ComponentModel.DataAnnotations.Schema.Table("doctor_current_status")]
    public class DoctorCurrentStatusModel
    {
        [Dapper.Contrib.Extensions.Key]
        [System.ComponentModel.DataAnnotations.Key]
        public long status_id { get; set; }

        public long dcode { get; set; }

        public string? tenant_code { get; set; }

        public string status { get; set; } = "OFF_DUTY";

        public string? remarks { get; set; }

        public DateTime? expected_return_time { get; set; }

        public long? updated_by { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;
    }
}

namespace Medico_Backend.Model
{
    // Alias namespace for backend compatibility if needed
    public class DoctorCurrentStatusModel : Booking.Models.DoctorCurrentStatusModel
    {
    }
}
