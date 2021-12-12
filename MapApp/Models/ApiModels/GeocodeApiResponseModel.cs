using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapApp.Models.ApiRequestModels
{
    public partial class GeocodeApiResponseModel
    {
        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("options")]
        public Options Options { get; set; }

        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("statuscode")]
        public long Statuscode { get; set; }

        [JsonProperty("copyright")]
        public Copyright Copyright { get; set; }

        [JsonProperty("messages")]
        public object[] Messages { get; set; }
    }

    public partial class Copyright
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("imageAltText")]
        public string ImageAltText { get; set; }
    }

    public partial class Options
    {
        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("thumbMaps")]
        public bool ThumbMaps { get; set; }

        [JsonProperty("ignoreLatLngInput")]
        public bool IgnoreLatLngInput { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("providedLocation")]
        public ProvidedLocation ProvidedLocation { get; set; }

        [JsonProperty("locations")]
        public Location[] Locations { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("adminArea6")]
        public string AdminArea6 { get; set; }

        [JsonProperty("adminArea6Type")]
        public string AdminArea6Type { get; set; }

        [JsonProperty("adminArea5")]
        public string AdminArea5 { get; set; }

        [JsonProperty("adminArea5Type")]
        public string AdminArea5Type { get; set; }

        [JsonProperty("adminArea4")]
        public string AdminArea4 { get; set; }

        [JsonProperty("adminArea4Type")]
        public string AdminArea4Type { get; set; }

        [JsonProperty("adminArea3")]
        public string AdminArea3 { get; set; }

        [JsonProperty("adminArea3Type")]
        public string AdminArea3Type { get; set; }

        [JsonProperty("adminArea1")]
        public string AdminArea1 { get; set; }

        [JsonProperty("adminArea1Type")]
        public string AdminArea1Type { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("geocodeQualityCode")]
        public string GeocodeQualityCode { get; set; }

        [JsonProperty("geocodeQuality")]
        public string GeocodeQuality { get; set; }

        [JsonProperty("dragPoint")]
        public bool DragPoint { get; set; }

        [JsonProperty("sideOfStreet")]
        public string SideOfStreet { get; set; }

        [JsonProperty("unknownInput")]
        public string UnknownInput { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("latLng")]
        public LatLng LatLng { get; set; }

        [JsonProperty("displayLatLng")]
        public LatLng DisplayLatLng { get; set; }
    }

    public partial class LatLng
    {
        [JsonProperty("lat")]
        public float Lat { get; set; }

        [JsonProperty("lng")]
        public float Lng { get; set; }
    }

    public partial class ProvidedLocation
    {
        [JsonProperty("street")]
        public string Street { get; set; }
    }
}
