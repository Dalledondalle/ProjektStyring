using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjektStyring.Models
{
    public class MaterialModel
    {
        [JsonProperty(PropertyName = "studentId")]
        public string StudentId { get; set; }

        [DisplayName("Navn på materialet")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [DisplayName("Antal")]
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [DisplayName("Yderliger kommentar")]
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        [JsonProperty(PropertyName = "addedDate")]
        public DateTime AddedDate { get; set; }

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
