using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace ProjektStyring.Models
{
    public class TaskModel
    {
        [DisplayName("ID")]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [DisplayName("Kunde")]
        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "instructorId")]
        public string InstructorId { get; set; }

        [DisplayName("Titel")]
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [DisplayName("Beskrivelse")]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [DisplayName("Link til Projekt")]
        [JsonProperty(PropertyName = "projectUrl")]
        public string ProjectUrl { get; set; }

        [JsonProperty(PropertyName = "blobs")]
        public ICollection<Blob> Blobs { get; set; }

        [JsonProperty(PropertyName = "createdTime")]
        public DateTime CreatedTime { get; set; }

        [DisplayName("Billeder")]
        public IFormFile File { get; set; }

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; } = false;

        [JsonProperty(PropertyName = "currentStatus")]
        public CurrentStatus CurrentStatus { get; set; }
    }
}

public enum CurrentStatus
{
    Bearbejdes,
    Afvist,
    Accepteret,
    Færdigt
}

