using MapApp.Models.ApiRequestModels;
using MapApp.Models.EF.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            Database.EnsureCreated();
 
        }

        public DbSet<Bus> Buses { get; set; }
        public DbSet<City> WayPoints { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<WayPointsSchedule> WayPointsSchedules { get; set; }
        public DbSet<Path> Paths { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        //}

        public List<Path> GetWayBetweenCities(string busId, List<string> cities)
        {
            List<Path> coordinates = new List<Path>();
            RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(cities);

            //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
            //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
          
            HttpResponseMessage response =  client.PostAsJsonAsync(
                "http://open.mapquestapi.com/directions/v2/route?key=iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI", routingRequest).Result;

            RoutingApiResponseModel responseModel =  response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;


            for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2)
            {
                coordinates.Add(new Path()
                {
                    Id= autoIncId,
                    BusId = busId,
                    Longtitude = responseModel._Route._Shape.ShapePoints[i],
                    Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                });
                autoIncId++;
            }


            //string testjson = await response.Content.ReadAsStringAsync();

          //  response.EnsureSuccessStatusCode();
           // var test = response.Headers.Location;
            
            return coordinates;

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
            new City(){
                Id = "1",
                Name = "Kyiv"
            },
            new City(){
                Id = "2",
                Name = "Kharkiv"
            },
            new City(){
                Id = "3",
                Name = "Svitlovodsk"
            });
            modelBuilder.Entity<Bus>().HasData(
            new Bus(){
                Id = "1",
                Operator = "Ivan",
                FromCityId = "1",
                ToCityId = "3"
            },
            new Bus()
            {
                Id = "2",
                Operator = "Dmitriy",
                FromCityId = "3",
                ToCityId = "2"
            },
            new Bus()
            {
                Id = "3",
                Operator = "David",
                FromCityId = "2",
                ToCityId = "3"
            });
            modelBuilder.Entity<Schedule>().HasData(
            new Schedule(){
                Id  = "1",
                BusId = "1",
                Day = DayOfWeek.Monday,
                
            },
            new Schedule()
            {
                Id = "2",
                BusId = "1",
                Day = DayOfWeek.Wednesday,

            },
            new Schedule()
            {
                Id = "3",
                BusId = "2",
                Day = DayOfWeek.Tuesday,

            },
            new Schedule()
            {
                Id = "4",
                BusId = "2",
                Day = DayOfWeek.Thursday,

            },
            new Schedule()
            {
                Id = "5",
                BusId = "3",
                Day = DayOfWeek.Friday,

            },
            new Schedule()
            {
                Id = "6",
                BusId = "3",
                Day = DayOfWeek.Saturday,

            });
            
            modelBuilder.Entity<WayPointsSchedule>().HasData(new WayPointsSchedule()
            {
                Id = "1",
                BusId = "3",
                CityId ="1",
                Sequence = 1,
                Time = DateTime.Now
            });

            List<Path> paths = new List<Path>();
            paths.AddRange(GetWayBetweenCities("1", new List<string>() { "Kyiv", "Svitlovodsk" }));
            paths.AddRange(GetWayBetweenCities("2", new List<string>() { "Svitlovodsk", "Kharkiv" }));
            paths.AddRange(GetWayBetweenCities("3", new List<string>() { "Kharkiv", "Kyiv", "Svitlovodsk" }));

            modelBuilder.Entity<Path>().HasData(paths);

            //modelBuilder.Entity<Path>().HasData(,await GetWayBetweenCities("2", new List<string>() { "Svitlovodsk", "Kharkiv" }));
            //modelBuilder.Entity<Path>().HasData(GetWayToCity("3", new List<string>() { "Kharkiv" , "Kyiv", "Svitlovodsk",}));
            //await GetWayToCity();
        }
    }
}
