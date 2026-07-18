using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamX.Shared.DTOs
{
    public class LicenseValidationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Active", "Expired", "Invalid"
        public DateTime? ExpiresAt { get; set; }
        public bool HardwareMatched { get; set; }
    }
}