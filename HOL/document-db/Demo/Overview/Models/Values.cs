using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Overview.Models
{
    public class Values
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
       
        public string name { get; set; }

        public string description { get; set; }
    }
}
