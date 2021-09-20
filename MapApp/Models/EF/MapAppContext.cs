using MapApp.Models.ApiRequestModels;
using MapApp.Models.EF.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapApp.Models.EF
{
    public class MapAppContext:DbContext
    {
        private static int autoIncId = 1;
        private static readonly HttpClient client = new HttpClient();
        public MapAppContext(DbContextOptions<MapAppContext> options) : base(options)
        {
            // Database.EnsureDeleted();
            //Database.EnsureCreated();

        }
        public MapAppContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }


        public DbSet<Bus> Buses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<WayPointsSchedule> WayPointsSchedules { get; set; }
        public DbSet<Path> Paths { get; set; }
        public DbSet<Coords> Coords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlServer("Server=DESKTOP-UJK1CNS;Database=MapAppDB;Trusted_Connection=True;");
            }
        }



        public List<Coords> GetWayBetweenCities(string pathId, string cityFrom, string cityTo)
        {
            List<Coords> coordinates = new List<Coords>();
            RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom , cityTo });

            //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
            //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
          
            HttpResponseMessage response =  client.PostAsJsonAsync(
                "http://open.mapquestapi.com/directions/v2/route?key=iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI", routingRequest).Result;

            RoutingApiResponseModel responseModel =  response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;


            for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2)
            {
                coordinates.Add(new Coords()
                {
                    Id = autoIncId,
                    PathId = pathId,
                    Longtitude = responseModel._Route._Shape.ShapePoints[i],
                    Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                });
                autoIncId++;
            }


            //string testjson = await response.Content.ReadAsStringAsync();

           // response.EnsureSuccessStatusCode();
           // var test = response.Headers.Location;
            
            return coordinates;

        }

        public City GetCityCoords(string id, string city)
        {
            GeocodeApiRequestModel geocodeRequest = new GeocodeApiRequestModel(city);
            HttpResponseMessage response = client.PostAsJsonAsync(
                "http://www.mapquestapi.com/geocoding/v1/address?key=iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI", geocodeRequest).Result;

            GeocodeApiResponseModel responseModel = response.Content.ReadFromJsonAsync< GeocodeApiResponseModel>().Result;

            return new City()
            {
                Id = id,
                Name = city,
                Longtitude = responseModel.Results[0].Locations[0].LatLng.Lng,
                Latitude = responseModel.Results[0].Locations[0].LatLng.Lat
            };

        }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
 
        
        //var test = GetCityCoords("Kyiv");
        //var t1 = test.Item1;
        //var t2 = test.Item2;
        modelBuilder.Entity<City>().HasData(
                GetCityCoords("1","Kyiv"), 
                GetCityCoords("2", "Kharkiv"), 
                GetCityCoords("3", "Svitlovodsk"),
                GetCityCoords("4", "Lutsk"),
                GetCityCoords("5", "Lviv"),
                GetCityCoords("6", "Ternopil"),
                GetCityCoords("7", "Sumy"),
                GetCityCoords("8", "Poltava"),
                GetCityCoords("9", "Kremenchuk"),

                GetCityCoords("10", "Moscow"),
                GetCityCoords("11", "Belgorod"),
                GetCityCoords("12", "Tula"),
                GetCityCoords("13", "Tambov"),
                GetCityCoords("14", "Penza"),
                GetCityCoords("15", "Smolensk"),
                GetCityCoords("16", "Bryansk"),
                GetCityCoords("17", "Ryazan"),
                GetCityCoords("18", "Tver"),

                GetCityCoords("19", "Dnipropetrovsk")
            );
            //modelBuilder.Entity<Bus>().HasData(
            //new Bus()
            //{
            //    Id = "1",
            //    Operator = "Ivan",

            //},
            //new Bus()
            //{
            //    Id = "2",
            //    Operator = "Dmitriy",

            //},
            //new Bus()
            //{
            //    Id = "3",
            //    Operator = "David",

            //});
            //modelBuilder.Entity<Schedule>().HasData(
            //new Schedule(){
            //    Id  = "1",
            //    BusId = "1",
            //    Day = DayOfWeek.Monday,

            //},
            //new Schedule()
            //{
            //    Id = "2",
            //    BusId = "1",
            //    Day = DayOfWeek.Wednesday,

            //},
            //new Schedule()
            //{
            //    Id = "3",
            //    BusId = "2",
            //    Day = DayOfWeek.Tuesday,

            //},
            //new Schedule()
            //{
            //    Id = "4",
            //    BusId = "2",
            //    Day = DayOfWeek.Thursday,

            //},
            //new Schedule()
            //{
            //    Id = "5",
            //    BusId = "3",
            //    Day = DayOfWeek.Friday,

            //},
            //new Schedule()
            //{
            //    Id = "6",
            //    BusId = "3",
            //    Day = DayOfWeek.Saturday,

            //});

            //modelBuilder.Entity<WayPointsSchedule>().HasData(
            //new WayPointsSchedule()
            //{
            //    Id = "1",
            //    BusId = "3",
            //    PathId = "1",
            //    Sequence = 1,
            //    Time = DateTime.Now
            //},
            //new WayPointsSchedule()
            //{
            //    Id = "2",
            //    BusId = "3",
            //    PathId = "2",
            //    Sequence = 2,
            //    Time = DateTime.Now
            //},
            // new WayPointsSchedule()
            // {
            //     Id = "3",
            //     BusId = "2",
            //     PathId = "3",
            //     Sequence = 1,
            //     Time = DateTime.Now
            // },
            //new WayPointsSchedule()
            //{
            //    Id = "4",
            //    BusId = "1",
            //    PathId = "2",
            //    Sequence = 1,
            //    Time = DateTime.Now
            //});

            //modelBuilder.Entity<Path>().HasData(
            //new Path(){
            //    Id = "1",
            //    CityFromId = "2",
            //    CityToId ="1"
            //},
            //new Path()
            //{
            //    Id = "2",
            //    CityFromId = "1",
            //    CityToId = "3"
            //},
            //new Path()
            //{
            //    Id = "3",
            //    CityFromId = "3",
            //    CityToId = "2"
            //});

            //List<Coords> coords = new List<Coords>();

            //coords.AddRange(GetWayBetweenCities("1", "Kharkiv", "Kyiv"));
            //coords.AddRange(GetWayBetweenCities("2", "Kyiv", "Svitlovodsk"));
            //coords.AddRange(GetWayBetweenCities("3", "Svitlovodsk", "Kharkiv"));

            //modelBuilder.Entity<Coords>().HasData(coords);

        }
    }
}
