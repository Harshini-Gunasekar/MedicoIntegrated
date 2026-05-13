using System.Collections.Generic;

namespace Booking.Models
{
    public class SlotInsertResponse
    {
        public List<object> inserted { get; set; } = new();
        public List<string> skipped { get; set; } = new();
        public List<object> failed { get; set; } = new();
    }
}
