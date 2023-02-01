using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ProjektStyring.Models
{
    public class ProjectModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }         

        [JsonProperty(PropertyName = "caseId")]
        [DisplayName("Sagsnummer")]                  // Case number as ID.
        public string CaseId { get; set; }

        [JsonProperty(PropertyName = "title")]
        [DisplayName("Titel")]
        public string Title { get; set; }           // Title of project. 

        [JsonProperty(PropertyName = "description")]
        [DisplayName("Beskrivelse")]
        public string Description { get; set; }     // Description of the project.

        [JsonProperty(PropertyName = "priority")]
        [DisplayName("Prioritet")]
        public string Priority { get; set; }        // Priority: 1 -10

        [JsonProperty(PropertyName = "departmentId")]
        [DisplayName("Afdeling")]
        public string DepartmentId { get; set; }      // Department of the project e.g. Tømrer, murer, elektriker.

        [JsonProperty(PropertyName = "startDate")]
        [DisplayName("Startdato")]
        public DateTime StartDate { get; set; }     // Creation date of project.

        [JsonProperty(PropertyName = "endDate")]
        [DisplayName("Slutdato")]
        public DateTime EndDate { get; set; }       // End date of project.

        [JsonProperty(PropertyName = "customerId")]
        [DisplayName("Kunde")]
        public string CustomerId { get; set; }      // CustomerId.

        [JsonProperty(PropertyName = "instructorId")]
        [DisplayName("Instruktør")]
        public string InstructorId { get; set; }    // InstructorId.

        [JsonProperty(PropertyName = "studentIdList")]
        [DisplayName("Elever")]
        public List<string> StudentIds { get { if (sids == null) return new List<string>(); else return sids; } set { sids = value; } }                       // Students of the day.
        private List<string> sids = new List<string>();

        [JsonProperty(PropertyName = "historyList")]
        [DisplayName("Historik")]
        public ICollection<HistoryEntry> History { get; set; }              // History of the project - which students were assigned on a given day.

        [JsonProperty(PropertyName = "materialList")]
        [DisplayName("Materialer")]
        public List<MaterialModel> Materials { get; set; }           // List of materials

        [JsonProperty(PropertyName = "documentationList")]
        [DisplayName("Dokumentation")]
        public List<DescriptiveItem> DocumentationList { get; set; } // List of documentation by students.

        [JsonProperty(PropertyName = "isComplete")]
        [DisplayName("Færdig")]
        public bool IsComplete { get; set; } = false;        // Is project completed?

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}