using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjektStyring.Models
{
    public class PersonModel
    {
        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        //[JsonProperty(PropertyName = "department")]
        //public string Department { get; set; }

        [JsonProperty(PropertyName = "Mail")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string FullName { get; set; }

        public string NameAndEmail { get { return FullName + " - " + Email; } }
    }
}
