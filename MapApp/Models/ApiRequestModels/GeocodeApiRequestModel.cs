using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapApp.Models.ApiRequestModels
{
    public class GeocodeApiRequestModel
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("options")]
        public Options Option { get; set; }

        public GeocodeApiRequestModel(string location)
        {
            Location = location;
        }

        public class Options
        {

            public bool ThumbMaps { get; set; }
            public Options()
            {
                ThumbMaps = false;
            }
        }
    }
}
