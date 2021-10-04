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
            //Database.EnsureDeleted();
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
        public DbSet<Country> Countries { get; set; }

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

        public City GetCityCoords(string id, string city, string countryId)
        {
            GeocodeApiRequestModel geocodeRequest = new GeocodeApiRequestModel(city);
            HttpResponseMessage response = client.PostAsJsonAsync(
                "http://www.mapquestapi.com/geocoding/v1/address?key=S0B3YTkcDSAWJx7JPKAdw0vw43A67nvH", geocodeRequest).Result;

            GeocodeApiResponseModel responseModel = response.Content.ReadFromJsonAsync<GeocodeApiResponseModel>().Result;

            return new City()
            {
                Id = id,
                Name = city,
                Longtitude = responseModel.Results[0].Locations[0].LatLng.Lng,
                Latitude = responseModel.Results[0].Locations[0].LatLng.Lat,
                CountryId = countryId
            };

        }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WayPointsSchedule>().Property(p => p.CityToArrivalTime).IsRequired(false).HasColumnType("Time");
            modelBuilder.Entity<WayPointsSchedule>().Property(p => p.CityFromDepartTime).IsRequired(false).HasColumnType("Time");

        //    modelBuilder.Entity<Country>().HasData(new Country()
        //    {
        //        Id = "1",
        //        Name ="Ukraine"
        //    },
        //    new Country()
        //    {
        //        Id = "2",
        //        Name = "Russia"
        //    },
        //    new Country()
        //    {
        //        Id = "3",
        //        Name = "Poland"
        //    });

        ////var test = GetCityCoords("Kyiv");
        ////var t1 = test.Item1;
        ////var t2 = test.Item2;
        //    modelBuilder.Entity<City>().HasData(
        //        GetCityCoords("1","Kyiv","1"), 
        //        GetCityCoords("2", "Kharkiv", "1"), 
        //        GetCityCoords("3", "Svitlovodsk", "1"),
        //        GetCityCoords("4", "Lutsk", "1"),
        //        GetCityCoords("5", "Lviv", "1"),
        //        GetCityCoords("6", "Ternopil", "1"),
        //        GetCityCoords("7", "Sumy", "1"),
        //        GetCityCoords("8", "Poltava", "1"),
        //        GetCityCoords("9", "Kremenchuk", "1"),
        //        GetCityCoords("19", "Dnipropetrovsk", "1"),

        //        GetCityCoords("10", "Moscow", "2"),
        //        GetCityCoords("11", "Belgorod", "2"),
        //        GetCityCoords("12", "Tula", "2"),
        //        GetCityCoords("13", "Tambov", "2"),
        //        GetCityCoords("14", "Penza", "2"),
        //        GetCityCoords("15", "Smolensk", "2"),
        //        GetCityCoords("16", "Bryansk", "2"),
        //        GetCityCoords("17", "Ryazan", "2"),
        //        GetCityCoords("18", "Tver", "2"),

        //        GetCityCoords("20", "Krakow", "3"),
        //        GetCityCoords("21", "Wroclaw", "3"),
        //        GetCityCoords("22", "Bydgoszcz", "3"),
        //        GetCityCoords("23", "Bialystok", "3"),
        //        GetCityCoords("24", "Rzeszow", "3"),
        //        GetCityCoords("25", "Poznan", "3"),
        //        GetCityCoords("26", "Plock", "3"),
        //        GetCityCoords("27", "Radom", "3"),
        //        GetCityCoords("28", "Kielce", "3"),
        //        GetCityCoords("29", "Lublin", "3")
        //    );
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
