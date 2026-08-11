using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.TelePhony
{
    public class TelephonyOptions
    {
        public const string SectionName = "Telephony";
        public string Provider { get; set; } = "Mock";
        public int RetryIntervalMinutes { get; set; } = 10;
        public int MaxAttempts { get; set; } = 12;
        public int QuietHoursStart { get; set; } = 21;
        public int QuietHoursEnd { get; set; } = 8;
    }
    public class ExotelOptions
    {
        public const string SectionName = "Exotel";
        public string AccountSid { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiToken { get; set; } = "";
        public string CallerId { get; set; } = "";
        public string Subdomain { get; set; } = "api.in.exotel.com";
        public string IvrAppId { get; set; } = "";
        public string WebhookSecret { get; set; } = "";
        public string StatusCallbackUrl { get; set; } = "";
    }
}
