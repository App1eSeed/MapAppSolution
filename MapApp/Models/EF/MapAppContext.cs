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
            Database.EnsureCreated();

        }
        public MapAppContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicesInOrder> ServicesInOrders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Transportation> Transportations { get; set; }
        public DbSet<TransportationWaypoint> TransportationWaypoints { get; set; }
        public DbSet<TransportationWaipointSeat> TransportationBusSeats { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<BusType> BusTypes{ get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<WayPointsSchedule> WayPointsSchedules { get; set; }
        public DbSet<Path> Paths { get; set; }
        public DbSet<Coords> Coords { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<News> News { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlServer("Server=DESKTOP-ERHD2RT;Database=MapAppDB;Trusted_Connection=True;");
            }
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
               // CountryId = countryId
            };

        }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Path>()
            .HasOne(f => f.CityFrom)
            .WithMany(c => c.CityFromPath)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Path>()
            .HasOne(f => f.CityTo)
            .WithMany(c => c.CityToPath)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TransportationWaypoint>()
            .HasOne(f => f.WayPointsSchedule)
            .WithMany(c => c.TransportationWaypoints)
            .OnDelete(DeleteBehavior.NoAction);



            modelBuilder.Entity<BusType>().HasData(new BusType()
            {
                Id = "1",
                Name = "CityBus",
                SeatsCount = 23
                
            },
            new BusType()
            {
                Id = "2",
                Name = "CountryBus",
                SeatsCount = 33

            },
            new BusType()
            {
                Id = "3",
                Name = "InternationalBus",
                SeatsCount = 50
            });

            modelBuilder.Entity<UserRole>().HasData(new UserRole()
            {
                Id = "1",
                Name = "User"
            });

            modelBuilder.Entity<Service>().HasData(new Service()
            {
                Id = "1",
                Name = "Animals, birds",
                Price = 85
            },
            new Service()
            {
                Id = "2",
                Name = "Apparatus",
                Price = 70
            },
            new Service()
            {
                Id = "3",
                Name = "Excess of stuff (more than 1 suitcase)",
                Price = 80
            });

            modelBuilder.Entity<OrderStatus>().HasData(new OrderStatus()
            {
                Id = "1",
                Name = "Not confirmed",
            },
            new Service()
            {
                Id = "2",
                Name = "Accepted",
            },
            new Service()
            {
                Id = "3",
                Name = "Declined",
            });


            //modelBuilder.Entity<Country>().HasData(new Country()
            //{
            //    Id = "804",
            //    Name = "Ukraine",
            //    CountryCode = "UA"
            //},
            //new Country()
            //{
            //    Id = "643",
            //    Name = "Russia"
            //    CountryCode = "RU"
            //},
            //new Country()
            //{
            //    Id = "616",
            //    Name = "Poland"
            //    CountryCode = "PL"
            //},
            //new Country()
            //{
            //    Id = "112",
            //    Name = "Belarus"
            //    CountryCode = "BY"
            //},
            //new Country()
            //{
            //    Id = "642",
            //    Name = "Romania"
            //    CountryCode = "RO"
            //},
            //new Country()
            //{
            //    Id = "100",
            //    Name = "Bulgaria"
            //    CountryCode = "BG"
            //},
            //new Country()
            //{
            //    Id = "276",
            //    Name = "Germany"
            //    CountryCode = "DE"
            //});

            ////var test = GetCityCoords("Kyiv");
            ////var t1 = test.Item1;
            ////var t2 = test.Item2;
            //modelBuilder.Entity<City>().HasData(
            //    GetCityCoords("1", "Kyiv", "1"),
            //    GetCityCoords("2", "Kharkiv", "1"),
            //    GetCityCoords("3", "Svitlovodsk", "1"),
            //    GetCityCoords("4", "Lutsk", "1"),
            //    GetCityCoords("5", "Lviv", "1"),
            //    GetCityCoords("6", "Ternopil", "1"),
            //    GetCityCoords("7", "Sumy", "1"),
            //    GetCityCoords("8", "Poltava", "1"),
            //    GetCityCoords("9", "Kremenchuk", "1"),
            //    GetCityCoords("19", "Dnipropetrovsk", "1"),

            //    GetCityCoords("10", "Moscow", "2"),
            //    GetCityCoords("11", "Belgorod", "2"),
            //    GetCityCoords("12", "Tula", "2"),
            //    GetCityCoords("13", "Tambov", "2"),
            //    GetCityCoords("14", "Penza", "2"),
            //    GetCityCoords("15", "Smolensk", "2"),
            //    GetCityCoords("16", "Bryansk", "2"),
            //    GetCityCoords("17", "Ryazan", "2"),
            //    GetCityCoords("18", "Tver", "2"),

            //    GetCityCoords("20", "Krakow", "3"),
            //    GetCityCoords("21", "Wroclaw", "3"),
            //    GetCityCoords("22", "Bydgoszcz", "3"),
            //    GetCityCoords("23", "Bialystok", "3"),
            //    GetCityCoords("24", "Rzeszow", "3"),
            //    GetCityCoords("25", "Poznan", "3"),
            //    GetCityCoords("26", "Plock", "3"),
            //    GetCityCoords("27", "Radom", "3"),
            //    GetCityCoords("28", "Kielce", "3"),
            //    GetCityCoords("29", "Lublin", "3"),

            //    GetCityCoords("30", "Zaporizhia", "1"),
            //    GetCityCoords("31", "Donetsk", "1"),
            //    GetCityCoords("32", "Mariupol", "1"),
            //    GetCityCoords("33", "Melitopol", "1"),
            //    GetCityCoords("34", "Kherson", "1"),
            //    GetCityCoords("35", "Mykolaiv", "1"),
            //    GetCityCoords("36", "Odesa", "1"),
            //    GetCityCoords("37", "Kropyvnytskyi", "1"),
            //    GetCityCoords("38", "Kryvyi Rih", "1"),
            //    GetCityCoords("39", "Vinnytsia", "1"),

            //    GetCityCoords("40", "Minsk", "4"),
            //    GetCityCoords("41", "Brest", "4"),
            //    GetCityCoords("42", "Mahilyow", "4"),
            //    GetCityCoords("43", "Homyel", "4"),
            //    GetCityCoords("44", "Orsha", "4"),
            //    GetCityCoords("45", "Vitebsk", "4"),
            //    GetCityCoords("46", "Hrodna", "4"),
            //    GetCityCoords("47", "Babruysk", "4"),
            //    GetCityCoords("48", "Lahuny", "4"),
            //    GetCityCoords("49", "Kobryn", "4"),

            //    GetCityCoords("50", "Bucharest", "5"),
            //    GetCityCoords("51", "Constanta", "5"),
            //    GetCityCoords("52", "Craiova", "5"),
            //    GetCityCoords("53", "Brasov", "5"),
            //    GetCityCoords("54", "Timisoara", "5"),
            //    GetCityCoords("55", "Cluj-Napoca", "5"),
            //    GetCityCoords("56", "Galati", "5"),
            //    GetCityCoords("57", "Bacau", "5"),
            //    GetCityCoords("58", "Satu Mare", "5"),
            //    GetCityCoords("59", "Oradea", "5"),

            //    GetCityCoords("60", "Sofia", "6"),
            //    GetCityCoords("61", "Plovdiv", "6"),
            //    GetCityCoords("62", "Stara Zagora", "6"),
            //    GetCityCoords("63", "Varna", "6"),
            //    GetCityCoords("64", "Burgas", "6"),
            //    GetCityCoords("65", "Ruse", "6"),
            //    GetCityCoords("66", "Pleven", "6"),
            //    GetCityCoords("67", "Slavotin", "6"),
            //    GetCityCoords("68", "Vidin", "6"),
            //    GetCityCoords("69", "Shumen", "6"),




            //    GetCityCoords("70", "Berlin", "7"),
            //    GetCityCoords("71", "Hamburg", "7"),
            //    GetCityCoords("72", "Munich", "7"),
            //    GetCityCoords("73", "Cologne", "7"),
            //    GetCityCoords("74", "Dresden", "7"),
            //    GetCityCoords("75", "Leipzig", "7"),
            //    GetCityCoords("76", "Hanover", "7"),
            //    GetCityCoords("77", "Bremen", "7"),
            //    GetCityCoords("78", "Kiel", "7"),
            //    GetCityCoords("79", "Rostock", "7"),
            //    GetCityCoords("80", "Oldenburg", "7"),
            //    GetCityCoords("81", "Bielefeld", "7"),
            //    GetCityCoords("82", "Erfurt", "7"),
            //    GetCityCoords("83", "Nuremberg", "7"),
            //    GetCityCoords("84", "Augsburg", "7"),
            //    GetCityCoords("85", "Stuttgart", "7"),
            //    GetCityCoords("86", "Wiesbaden", "7"),
            //    GetCityCoords("87", "Bonn", "7"),
            //    GetCityCoords("88", "Essen", "7"),
            //    GetCityCoords("89", "Dortmund", "7")


            //);
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
