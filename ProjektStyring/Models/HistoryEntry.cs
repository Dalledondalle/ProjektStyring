using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjektStyring.Models
{
    public class HistoryEntry
    {
        [JsonProperty(PropertyName = "studentId")]
        public string StudentId { get; set; }

        [JsonProperty(PropertyName = "loginTime")]
        public DateTime LoginTime { get; set; }

        [JsonProperty(PropertyName = "logoutTime")]
        public DateTime LogoutTime { get; set; }
    }
}
