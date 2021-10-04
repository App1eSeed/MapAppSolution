using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapApp.Models.ApiRequestModels
{
    public class RoutingApiResponseModel
    {
        [JsonPropertyName("route")]
        public Route _Route { get; set; }
        public class Route
        {
            [JsonPropertyName("shape")]
            public Shape _Shape { get; set; }

            [JsonPropertyName("distance")]
            public float Distance { get; set; }

            [JsonPropertyName("time")]
            public int Time { get; set; }
            public class Shape
            {
                [JsonPropertyName("shapePoints")]
                public float[] ShapePoints { get; set; }
            }
        }
    }
}
