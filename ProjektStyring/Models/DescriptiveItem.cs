using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ProjektStyring.Models
{
    public class DescriptiveItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "studentId")]
        public string StudentId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        
        //public DateTime StartDate { get; set; }
        public DateTime StartDate { get { return startDate.AddHours(2); } set { startDate = value; } }
        [JsonProperty(PropertyName = "startDate")]
        private DateTime startDate { get; set; }


        [JsonProperty(PropertyName = "blobs")]
        public ICollection<Blob> Blobs { get; set; }

        [JsonIgnore]
        public IFormFile File { get; set; }

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
