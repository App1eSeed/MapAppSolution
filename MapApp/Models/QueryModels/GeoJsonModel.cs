using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.QueryModels
{
    public class GeoJsonModel
    {
        [JsonConstructor]
        public GeoJsonModel(
            [JsonProperty("type")] string type,
            [JsonProperty("properties")] Properties properties,
            [JsonProperty("geometry")] Geometry geometry
        )
        {
            this.Type = type;
            this.Properties = properties;
            this.Geometry = geometry;
        }
        public GeoJsonModel()
        {

        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }
    }
    public class Properties
    {
        [JsonConstructor]
        public Properties(
            [JsonProperty("name")] string name,
            [JsonProperty("show_on_map")] bool showOnMap
        )
        {
            this.Name = name;
            this.ShowOnMap = showOnMap;
        }

        public Properties()
        {

        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("show_on_map")]
        public bool ShowOnMap { get; set; }
    }

    public class Geometry
    {
        [JsonConstructor]
        public Geometry(
            [JsonProperty("type")] string type,
            [JsonProperty("coordinates")] List<float> coordinates
        )
        {
            this.Type = type;
            this.Coordinates = coordinates;
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public List<float> Coordinates { get; set; }
    }

   
}


