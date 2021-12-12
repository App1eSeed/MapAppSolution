using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapApp.Models.ApiRequestModels
{
    
    public class Locations
    {
        public LatLng latLng { get; set; }
    }
    public class RoutingApiRequestModel
    {
        [JsonPropertyName("locations")]
        public List<Locations> Locations { get; set; }
        [JsonPropertyName("options")]
        public Options Option { get; set; }
        public RoutingApiRequestModel(List<Locations> locations)
        {
            Locations = new List<Locations>();
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
            Option = new Options();
        }

       
        public class Options
        {

            [JsonPropertyName("avoids")]
            public string[] Avoids { get; set; }
            [JsonPropertyName("avoidTimedConditions")]
            public bool AvoidTimedConditions { get; set; }
            [JsonPropertyName("doReverseGeocode")]
            public bool DoReverseGeocode { get; set; }
            [JsonPropertyName("shapeFormat")]
            public string ShapeFormat { get; set; }
            [JsonPropertyName("generalize")]
            public int Generalize { get; set; }
            [JsonPropertyName("routeType")]
            public string RouteType { get; set; }
            [JsonPropertyName("timeType")]
            public int TimeType { get; set; }
            [JsonPropertyName("locale")]
            public string Locale { get; set; }
            [JsonPropertyName("unit")]
            public string Unit { get; set; }
            [JsonPropertyName("enhancedNarrative")]
            public bool EnhancedNarrative { get; set; }
            [JsonPropertyName("drivingStyle")]
            public int DrivingStyle { get; set; }
            [JsonPropertyName("highwayEfficiency")]
            public double HighwayEfficiency { get; set; }

            public Options()
            {
                Avoids = new string[] { };
                AvoidTimedConditions = false;
                DoReverseGeocode = false;
                ShapeFormat = "raw";
                Generalize = 0;
                RouteType = "fastest";
                TimeType = 1;
                Locale = "en_US";
                Unit = "k";
                EnhancedNarrative = false;
                DrivingStyle = 2;
                HighwayEfficiency = 21.0;



            }

            private class OptionBulider
            {
                private Options options;

                public void AddAvoids(string[] avoids)
                {

                }
                public void AddAvoidTimeConditions()
                {

                }
                public void AddShapeFormat()
                {

                }
                public void AddGeneralize()
                {

                }
            }


        }
    }

}