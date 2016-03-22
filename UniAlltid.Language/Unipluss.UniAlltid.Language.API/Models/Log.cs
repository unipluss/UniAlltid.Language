using System;

namespace UniAlltid.Language.API.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string KeyId { get; set; }
        public string Lang { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime Timestamp { get; set; }
        public string Customer { get; set; }
        public string UpdatedBy { get; set; }
    }
}
