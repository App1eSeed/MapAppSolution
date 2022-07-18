using MapApp.Models.ApiRequestModels;
using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;

namespace MapApp
{
    public static class DbGenerator
    {
        private static List<City> capitalsCities;
        private static List<City> adminCities;
        private static List<City> minorCities;
        private static List<City> commonCities;
        private static int[] departMinutes;
        private static List<Path> allPaths;
        private static readonly HttpClient client = new HttpClient();

        private static int lastInsertedCoordsId { get; set; }
        static DbGenerator()
        {
            departMinutes = new int[4] { 0, 15, 30, 45 };

            using var context = new MapAppContext();

            capitalsCities = context.Cities.Where(c => c.Capital == Capital.primary).ToList();
            adminCities = context.Cities.Where(c => c.Capital == Capital.admin).ToList();
            minorCities = context.Cities.Where(c => c.Capital == Capital.minor).ToList();
            commonCities = context.Cities.Where(c => c.Capital == Capital.none).ToList();

        }

        public static void Generate(float searchRange = 0.5f, int stopTime = 15, int busCountGen = 1, int wayPointsCount = 6)
        {


            using (var context = new MapAppContext())
            {
                for (int i = 0; i < busCountGen; i++)
                {
                    allPaths = context.Paths.ToList();

                    if (context.Coords.OrderBy(p => p.Id).LastOrDefault() != null)
                    {
                        lastInsertedCoordsId = context.Coords.OrderBy(p => p.Id).LastOrDefault().Id;
                    }
                    else
                    {
                        lastInsertedCoordsId = 1;
                    }

                    var DataGen = CreateData(searchRange, stopTime, wayPointsCount, lastInsertedCoordsId);
                    var ClonnedData = CloneWaypointsOnTimeIntervals(DataGen.Bus, DataGen.WayPointsSchedules,DataGen.Schedules);



                    context.Buses.Add(DataGen.Bus);
                    context.Buses.AddRange(ClonnedData.ClonnedBuses);

                    context.Paths.AddRange(DataGen.Paths);

                    context.WayPointsSchedules.AddRange(DataGen.WayPointsSchedules);
                    context.WayPointsSchedules.AddRange(ClonnedData.WayPointsSchedules);
                                    
                    context.Schedules.AddRange(DataGen.Schedules);
                    context.Schedules.AddRange(ClonnedData.Schedules);

                    context.Coords.AddRange(DataGen.Coords);

                    context.Database.OpenConnection();
                    try
                    {
                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords ON;");
                        context.SaveChanges();
                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Coords OFF;");
                    }
                    finally
                    {
                        context.Database.CloseConnection();
                    }
                }
            }

        }

        public static void GenerateTransportations(int numberOfDays = 30)
        {
            using (var context = new MapAppContext())
            {
                var Buses = context.Buses.ToList();
                //разница в днях до начала следующей поездки (Schedules) нужно считать но поставил 2 потомучто пока что везде 2!!
                int differensInDays = 2;
                //значение нужно считать так как могут быть разные количества точек у каждого маршрута но так как генерю по 6 то работает!!!
                int waypointsCount = 6;

                var BusSchedules = (from bus in context.Buses
                                    join busType in context.BusTypes on bus.BusTypeId equals busType.Id
                                    join waypoint in context.WayPointsSchedules on bus.Id equals waypoint.BusId
                                    join waypointSchedule in context.Schedules on waypoint.Id equals waypointSchedule.WayPointsScheduleId
                                    join path in context.Paths on waypoint.PathId equals path.Id
                                    join cityFrom in context.Cities on path.CityFromId equals cityFrom.Id
                                    join cityTo in context.Cities on path.CityToId equals cityTo.Id
                                    orderby bus.Id, waypoint.Sequence,waypointSchedule.DepartDay
                                    select new
                                    {
                                        BusId = bus.Id,
                                        WaypointId = waypoint.Id,
                                        DepartTime = waypoint.CityFromDepartTime,
                                        ArrivalTime = waypoint.CityToArrivalTime,
                                        ArrivalDay = waypointSchedule.ArrivalDay,
                                        DepartDay = waypointSchedule.DepartDay,
                                        Price = waypoint.Price,
                                        SeatsNumber = busType.SeatsCount
                                    }).ToList();

                List<Transportation> transportations = new List<Transportation>();
                List<TransportationWaypoint> transportationWaypoints = new List<TransportationWaypoint>();
                List<TransportationWaipointSeat> transportationWaipointSeats = new List<TransportationWaipointSeat>();

                foreach (var bus in Buses)
                {
                    if (bus.Transportations.Count == 0)
                    {

                        for (int i = 0; i < 1; i++)
                        {
                            Transportation transportation = new Transportation()
                            {
                                Id = Guid.NewGuid().ToString(),
                                BusId = bus.Id
                            };
                            int counter = 1;
                            transportations.Add(transportation);//Проверить!
                            foreach (var busSchedule in BusSchedules.Where(s => s.BusId == bus.Id))
                            {
                               

                                TransportationWaypoint transWaypoint;
                                transportationWaypoints.Add(transWaypoint = new TransportationWaypoint()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    TransportationId = transportation.Id,
                                    WayPointsScheduleId = busSchedule.WaypointId,
                                    Price = busSchedule.Price
                                });

                                for (int j = 1; j <= busSchedule.SeatsNumber; j++)
                                {
                                    transportationWaipointSeats.Add(new TransportationWaipointSeat()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        TransportationWaypointId = transWaypoint.Id,
                                        OrderId = null,
                                        IsTaken = false,
                                        SeatNumber = j
                                    });
                                }

                                if ((counter - 1) % waypointsCount == 0)
                                {
                                    DateTime depDate = DateTime.Now.AddDays(busSchedule.DepartDay - DateTime.Now.DayOfWeek + 7 * i);
                                    transportation.DepartDate = new DateTime(depDate.Year,depDate.Month,depDate.Day);
                                    transportation.DepartTime = busSchedule.DepartTime;

                                }
                                if (counter % waypointsCount == 0)
                                {
                                    
                                    DateTime arrDate = DateTime.Now.AddDays(Math.Abs(busSchedule.ArrivalDay - DateTime.Now.DayOfWeek) + 7 * i);
                                    transportation.ArrivalDate = new DateTime(arrDate.Year,arrDate.Month,arrDate.Day);
                                    transportation.ArrivalTime = busSchedule.ArrivalTime;
                                    transportations.Add(transportation);

                                    transportation = new Transportation()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        BusId = bus.Id
                                    };
                                }
                                counter++;
                           
                            }

                        }
                    }

                }



                context.Transportations.AddRange(transportations);
                context.TransportationWaypoints.AddRange(transportationWaypoints);
                context.TransportationBusSeats.AddRange(transportationWaipointSeats);
                context.SaveChanges();
            }
        }
        private static DataGen CreateData(float searchRange, int stopTime, int pathCount, int LocalLastInsertedCoordsId)
        {
            var dataGen = new DataGen();
            Random r = new Random();

            Bus instBus = new Bus()
            {
                Id = Guid.NewGuid().ToString("N"),
                Operator = "Rand " + r.Next(1, 10),
                BusTypeId = "2"
            };

            //Generated Bus
            dataGen.Bus = instBus;


            City startCity = GetDepCityByType(GetDepArrCityType());
            City endCity = GetArrCityByType(GetDepArrCityType(), startCity.Latitude, startCity.Longtitude, pathCount, searchRange * 2);

            instBus.FromCity = startCity.Name;
            instBus.ToCity = endCity.Name;

            float latDiff = endCity.Latitude - startCity.Latitude,
                  lngDiff = endCity.Longtitude - startCity.Longtitude;

            List<City> excludedCities = new List<City>();
            excludedCities.Add(startCity);
            excludedCities.Add(endCity);

            City city1 = startCity;
            City city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, 1, pathCount, excludedCities, searchRange);

            List<Path> paths = new List<Path>();
            List<WayPointsSchedule> wayPoints = new List<WayPointsSchedule>();
            List<DayOfWeek> differentDays = new List<DayOfWeek>();
            DayOfWeek departDay = (DayOfWeek)r.Next(0, 7);
            DayOfWeek arrivalDay = departDay;
            differentDays.Add(departDay);

            for (int j = 0; j < pathCount; j++)
            {
                Path path = new Path();


                if (j == pathCount - 1)
                {
                    if (j % 2 == 0)
                    {
                        city2 = endCity;

                        var check = allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
                        check.AddRange(paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


                        if (check.Count > 0)
                        {
                            paths.Add(check[0]);
                            path = check[0];
                        }
                        else
                        {
                            path = new Path()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CityFromId = city1.Id,
                                CityToId = city2.Id
                            };

                            var CoordsBetweenCities = GetWayBetweenCities
                                (ref path,
                                city1,
                                city2,
                                LocalLastInsertedCoordsId
                                );
                            if (CoordsBetweenCities != null)
                            {
                                dataGen.Coords.AddRange(CoordsBetweenCities);
                            }
                            else
                            {
                                return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                            }
                            dataGen.Paths.Add(path);
                            paths.Add(path);
                            LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                        }
                    }
                    else
                    {
                        city1 = endCity;

                        var check = allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
                        check.AddRange(paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

                        if (check.Count > 0)
                        {
                            paths.Add(check[0]);
                            path = check[0];
                        }
                        else
                        {
                            path = new Path()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CityFromId = city2.Id,
                                CityToId = city1.Id
                            };


                            var CoordsBetweenCities = GetWayBetweenCities
                                (ref path,
                                city2,
                                city1,
                                LocalLastInsertedCoordsId
                                );
                            if (CoordsBetweenCities != null)
                            {
                                dataGen.Coords.AddRange(CoordsBetweenCities);
                            }
                            else
                            {
                                return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                            }
                            dataGen.Paths.Add(path);
                            paths.Add(path);
                            LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                        }
                    }
                }
                else if (j % 2 == 0)
                {

                    city2 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, excludedCities, searchRange);

                    excludedCities.Add(city2);

                    var check = allPaths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList();
                    check.AddRange(paths.Where(p => p.CityFromId == city1.Id && p.CityToId == city2.Id).ToList());


                    if (check.Count > 0)
                    {
                        paths.Add(check[0]);
                        path = check[0];
                    }
                    else
                    {
                        path = new Path()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            CityFromId = city1.Id,
                            CityToId = city2.Id
                        };

                        var CoordsBetweenCities = GetWayBetweenCities
                                 (ref path,
                                 city1,
                                 city2,
                                 LocalLastInsertedCoordsId
                                 );
                        if (CoordsBetweenCities != null)
                        {
                            dataGen.Coords.AddRange(CoordsBetweenCities);
                        }
                        else
                        {
                            return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                        }
                        dataGen.Paths.Add(path);
                        paths.Add(path);
                        LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;

                    }
                }
                else
                {

                    city1 = GetWayNodeByType(GetNodeCityType(), startCity.Latitude, startCity.Longtitude, latDiff, lngDiff, j + 1, pathCount, excludedCities, searchRange);


                    excludedCities.Add(city1);

                    var check = allPaths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList();
                    check.AddRange(paths.Where(p => p.CityFromId == city2.Id && p.CityToId == city1.Id).ToList());

                    if (check.Count > 0)
                    {
                        paths.Add(check[0]);
                        path = check[0];
                    }
                    else
                    {
                        path = new Path()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            CityFromId = city2.Id,
                            CityToId = city1.Id
                        };

                        var CoordsBetweenCities = GetWayBetweenCities
                                 (ref path,
                                 city2,
                                 city1,
                                 LocalLastInsertedCoordsId
                                 );
                        if (CoordsBetweenCities != null)
                        {
                            dataGen.Coords.AddRange(CoordsBetweenCities);
                        }
                        else
                        {
                            return CreateData(searchRange, stopTime, pathCount, LocalLastInsertedCoordsId - dataGen.Coords.Count);
                        }
                        dataGen.Paths.Add(path);
                        paths.Add(path);
                        LocalLastInsertedCoordsId = lastInsertedCoordsId + dataGen.Coords.Count;
                        
                    }

                }

                WayPointsSchedule wayPoint;
                Schedule schedule;

                if (wayPoints.Count == 0)
                {
                    var departTime = new TimeSpan(r.Next(6, 19), departMinutes[r.Next(0, 4)], 0);

                    dataGen.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        BusId = instBus.Id,
                        Sequence = j + 1,
                        PathId = path.Id,
                        Price = r.Next(51,120),
                        CityFromDepartTime = departTime,
                        CityToArrivalTime = new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0),
                        CityFromDepartTimeInSec = (int)departTime.TotalSeconds,
                        CityToArrivalTimeInSec = (int)new TimeSpan((departTime + TimeSpan.FromSeconds(path.Time)).Hours, (departTime + TimeSpan.FromSeconds(path.Time)).Minutes, 0).TotalSeconds
                    });         

                    if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                    {
                        arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                        differentDays.Add(arrivalDay);
                    }

                    dataGen.Schedules.Add(schedule = new Schedule()
                    {
                        WayPointsScheduleId = wayPoint.Id,
                        ArrivalDay = arrivalDay,
                        DepartDay = departDay
                    });
                }
                else
                {
                    TimeSpan departTime = wayPoints[j - 1].CityToArrivalTime + TimeSpan.FromMinutes(stopTime); // TimeSpan.FromSeconds(path.Time)
                    var arrivalTime = departTime + TimeSpan.FromSeconds(path.Time);

                    departDay = arrivalDay;

                    dataGen.WayPointsSchedules.Add(wayPoint = new WayPointsSchedule()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        BusId = instBus.Id,
                        Sequence = j + 1,
                        PathId = path.Id,
                        Price = r.Next(51, 120),
                        CityFromDepartTime = new TimeSpan(departTime.Hours, departTime.Minutes, 0),
                        CityToArrivalTime = new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0),
                        CityFromDepartTimeInSec = (int)new TimeSpan(departTime.Hours, departTime.Minutes, 0).TotalSeconds,
                        CityToArrivalTimeInSec = (int)new TimeSpan(arrivalTime.Hours, arrivalTime.Minutes, 0).TotalSeconds
                    });

                    if (departTime.Days > 0)
                    {
                        departDay = (DayOfWeek)(((int)departDay + 1) % 7);
                    }

                    if (arrivalTime.Days > 0)
                    {
                        arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                        differentDays.Add(arrivalDay);
                    }

                    //if (wayPoint.CityToArrivalTime.TotalSeconds < wayPoint.CityFromDepartTime.TotalSeconds)
                    //{
                    //    arrivalDay = (DayOfWeek)(((int)departDay + 1) % 7);
                    //    differentDays.Add(arrivalDay);
                    //}

                    dataGen.Schedules.Add(schedule = new Schedule()
                    {
                        WayPointsScheduleId = wayPoint.Id,
                        ArrivalDay = arrivalDay,
                        DepartDay = departDay
                    });
                }

                wayPoint.Schedules.Add(schedule);

                wayPoints.Add(wayPoint);
            }
            switch (differentDays.Count)
            {
                case 1:
                    for (int j = 0; j < 3; j++)
                    {
                        foreach (var wayPoint in wayPoints)
                        {
                            dataGen.Schedules.Add(new Schedule()
                            {
                                WayPointsScheduleId = wayPoint.Id,
                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                            });
                        }
                    }
                    break;
                case 2:
                    for (int j = 0; j < 2; j++)
                    {
                        foreach (var wayPoint in wayPoints)
                        {
                            dataGen.Schedules.Add(new Schedule()
                            {
                                WayPointsScheduleId = wayPoint.Id,
                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                            });
                        }
                    }
                    break;
                case 3:
                    for (int j = 0; j < 1; j++)
                    {
                        foreach (var wayPoint in wayPoints)
                        {
                            dataGen.Schedules.Add(new Schedule()
                            {
                                WayPointsScheduleId = wayPoint.Id,
                                ArrivalDay = (DayOfWeek)(((int)wayPoint.Schedules[0].ArrivalDay + differentDays.Count * (j + 1)) % 7),
                                DepartDay = (DayOfWeek)(((int)wayPoint.Schedules[0].DepartDay + differentDays.Count * (j + 1)) % 7)
                            });
                        }
                    }
                    break;
                default:
                    break;
            }

            return dataGen;
        }
        private static DataGen CloneWaypointsOnTimeIntervals(Bus bus, List<WayPointsSchedule> wayPointsSchedules,List<Schedule> schedules,int clonesCount = 6, int interval = 3)
        {
            List<Bus> clonedBuses = new List<Bus>();
            List<WayPointsSchedule> clonedWaypoints = new List<WayPointsSchedule>();
            List<Schedule> clonedSchedules = new List<Schedule>();

            for (int i = 1; i <= clonesCount; i++)
            {               
                var newBus = new Bus()
                {
                    Id = Guid.NewGuid().ToString(),
                    FromCity = bus.FromCity,
                    ToCity = bus.ToCity,
                    Operator = bus.Operator,
                    BusTypeId = bus.BusTypeId
                };
                clonedBuses.Add(newBus);
                foreach (var waypoint in wayPointsSchedules)
                {
                    var newWaypoint = new WayPointsSchedule()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PathId = waypoint.PathId,
                        BusId = newBus.Id,
                        CityFromDepartTimeInSec = waypoint.CityFromDepartTimeInSec,
                        CityToArrivalTimeInSec = waypoint.CityToArrivalTimeInSec,
                        Price = waypoint.Price,
                        Sequence = waypoint.Sequence,
                        CityFromDepartTime = new TimeSpan(
                            (waypoint.CityFromDepartTime + new TimeSpan(interval * i, 0, 0)).Hours, 
                            waypoint.CityFromDepartTime.Minutes, 
                            waypoint.CityFromDepartTime.Seconds
                            ),
                        CityToArrivalTime = new TimeSpan(
                            (waypoint.CityToArrivalTime + new TimeSpan(interval * i, 0, 0)).Hours,
                            waypoint.CityToArrivalTime.Minutes,
                            waypoint.CityToArrivalTime.Seconds
                            )                      
                    };
                    clonedWaypoints.Add(newWaypoint);

                    foreach (var schedule in schedules.Where(s => s.WayPointsScheduleId == waypoint.Id))
                    {
                        var newSchedule = new Schedule()
                        {
                            Id = Guid.NewGuid().ToString(),
                            WayPointsScheduleId = newWaypoint.Id,
                            ArrivalDay = schedule.ArrivalDay,
                            DepartDay = schedule.DepartDay
                        };
                        //NEED FIX 6-0 days
                        if ((waypoint.CityFromDepartTime + new TimeSpan(interval * i, 0, 0)).Days > 0)
                        {
                            newSchedule.DepartDay = (DayOfWeek)(((int)schedule.DepartDay + 1) % 7);
                        }
                        if ((waypoint.CityToArrivalTime + new TimeSpan(interval * i, 0, 0)).Days > 0)
                        {
                            newSchedule.ArrivalDay = (DayOfWeek)(((int)schedule.ArrivalDay + 1) % 7);
                        }
                        clonedSchedules.Add(newSchedule);
                    }
                }                       
            }
            return new DataGen()
            {
                ClonnedBuses = clonedBuses,
                WayPointsSchedules = clonedWaypoints,
                Schedules = clonedSchedules
            };
        }
        private static City GetDepCityByType(Capital type)
        {
            Random r = new Random();
            switch (type)
            {
                case Capital.primary:
                    return capitalsCities[r.Next(0, capitalsCities.Count)];
                case Capital.admin:
                    return adminCities[r.Next(0, adminCities.Count)];
                case Capital.minor:
                    return minorCities[r.Next(0, minorCities.Count)];
                case Capital.none:
                    return commonCities[r.Next(0, commonCities.Count)];
                default:
                    return new City();
            }
        }
        private static City GetArrCityByType(Capital type, float latFrom, float lngFrom, int wayPointsCount, float extension)
        {
            List<City> sortedCities = new List<City>();
            float limitation = 1f * wayPointsCount;
            Random r = new Random();

            switch (type)
            {
                case Capital.primary:
                    sortedCities = capitalsCities;
                    break;
                case Capital.admin:
                    sortedCities = adminCities;
                    break;
                case Capital.minor:
                    sortedCities = minorCities;
                    break;
                case Capital.none:
                    sortedCities = commonCities;
                    break;
                default:
                    break;

            }

            //sortedCities = sortedCities.Where(sc =>
            //    Math.Abs((latFrom + lngFrom) - (sc.Latitude + sc.Longtitude)) < limitation
            //).OrderByDescending(sc =>sc.Latitude).ThenByDescending(sc =>sc.Longtitude).Take(5).ToList();

            sortedCities = sortedCities.Where(sc =>
                ((sc.Latitude >= latFrom + limitation - extension
                && sc.Latitude <= latFrom + limitation + extension)
                ||
                (sc.Latitude >= latFrom - limitation - extension
                && sc.Latitude <= latFrom - limitation + extension))
                &&
                ((sc.Longtitude >= lngFrom + limitation - extension
                && sc.Longtitude <= lngFrom + limitation + extension)
                ||
                (sc.Longtitude >= lngFrom - limitation - extension
                && sc.Longtitude <= lngFrom - limitation + extension))
            ).ToList();



            if (sortedCities.Count == 0)
            {
                return GetArrCityByType(GetDepArrCityType(), latFrom, lngFrom, wayPointsCount, extension * 1.05f);
            }
            else
            {
                return sortedCities[r.Next(0, sortedCities.Count)];
            }

        }
        private static City GetWayNodeByType(Capital type, float latFrom, float lngFrom, float latDiff, float lngDiff, int iteration, int wayPointsCount, List<City> excludedCities, float extension)
        {
            List<City> sortedCities = new List<City>();
            Random r = new Random();

            switch (type)
            {
                case Capital.primary:
                    sortedCities = capitalsCities;
                    break;
                case Capital.admin:
                    sortedCities = adminCities;
                    break;
                case Capital.minor:
                    sortedCities = minorCities;
                    break;
                case Capital.none:
                    sortedCities = commonCities;
                    break;
                default:
                    break;

            }


            sortedCities = sortedCities.Where(sc =>
                sc.Latitude >= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
                && sc.Latitude <= latFrom + (latDiff / wayPointsCount) * (latDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(lngDiff) / Math.Abs(latDiff))
                && sc.Longtitude >= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? (iteration - 1) : iteration) - extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
                && sc.Longtitude <= lngFrom + (lngDiff / wayPointsCount) * (lngDiff > 0 ? iteration : (iteration - 1)) + extension * (Math.Abs(latDiff) / Math.Abs(lngDiff))
            ).ToList();


            if (sortedCities.Count == 0)
            {
                return GetWayNodeByType(GetDepArrCityType(), latFrom, lngFrom, latDiff, lngDiff, iteration, wayPointsCount, excludedCities, extension * 1.02f);
            }
            else
            {
                City generatedCity = sortedCities[r.Next(0, sortedCities.Count)];
                if (excludedCities.Contains(generatedCity))
                {
                    return GetWayNodeByType(GetDepArrCityType(), latFrom, lngFrom, latDiff, lngDiff, iteration, wayPointsCount, excludedCities, extension * 1.01f);
                }
                return generatedCity;
            }
        }
        private static List<Coords> GetWayBetweenCities(ref Path path, City cityFrom, City cityTo, int lastInsertedId)
        {
            List<Coords> coordinates = new List<Coords>();
            //RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<string>() { cityFrom.Name, cityTo.Name });

            RoutingApiRequestModel routingRequest = new RoutingApiRequestModel(new List<Locations>()
                {

                    new Locations(){latLng = new LatLng(){ Lat = cityFrom.Latitude,Lng = cityFrom.Longtitude }},
                    new Locations(){latLng =  new LatLng(){ Lat = cityTo.Latitude, Lng = cityTo.Longtitude }}

                });


            //string jsonString = JsonSerializer.Serialize<RoutingRequest>(routingRequest);
            //var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsJsonAsync(
                "http://open.mapquestapi.com/directions/v2/route?key=S0B3YTkcDSAWJx7JPKAdw0vw43A67nvH", routingRequest).Result;

            RoutingApiResponseModel responseModel = response.Content.ReadFromJsonAsync<RoutingApiResponseModel>().Result;

            int skipPoints = 1;
            if (responseModel._Route._Shape != null)
            {


                for (int i = 0; i < responseModel._Route._Shape.ShapePoints.Length; i += 2 * skipPoints)
                {
                    coordinates.Add(new Coords()
                    {
                        Id = lastInsertedId + i / (2 * skipPoints) + 1,
                        PathId = path.Id,
                        Longtitude = responseModel._Route._Shape.ShapePoints[i],
                        Latitude = responseModel._Route._Shape.ShapePoints[i + 1]
                    });

                }
                //if (!coordinates.Exists(c => c.Id == lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1))
                //{
                //    coordinates.Add(new Coords()
                //    {
                //        Id = lastInsertedId + (responseModel._Route._Shape.ShapePoints.Length - 2) / (2 * skipPoints) + 1,
                //        PathId = path.Id,
                //        Longtitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 2],
                //        Latitude = responseModel._Route._Shape.ShapePoints[responseModel._Route._Shape.ShapePoints.Length - 1]
                //    });
                //}
            }
            else
            {

                return null;
            }


            coordinates.OrderBy(c => c.Id).ToList();

            path.Distance = responseModel._Route.Distance;
            path.Time = responseModel._Route.Time;
            path.CoordsCount = responseModel._Route._Shape.ShapePoints.Length;


            //string testjson = await response.Content.ReadAsStringAsync();

            // response.EnsureSuccessStatusCode();
            // var test = response.Headers.Location;

            return coordinates;

        }
        private static Capital GetDepArrCityType()
        {
            Random r = new Random();
            int number = r.Next(0, 101);
            if (number <= -10)//fix
            {
                return Capital.none;
            }
            else if ( number <= -10)
            {
                return Capital.minor;
            }
            else if (/*number > 25 && number <= 65*/number > -1 && number <= 105)
            {
                return Capital.admin;
            }
            else
            {
                return Capital.primary;
            }
        }
        private static Capital GetNodeCityType()
        {
            Random r = new Random();
            int number = r.Next(0, 101);
            if (/*number <= 40*/number <= -1)
            {
                return Capital.none;
            }
            else if (/*number > 40 && number <= 75*/number <= -1)
            {
                return Capital.minor;
            }
            else if (/*number > 75 && number <= 95*/number > 0 && number <= 101)
            {
                return Capital.admin;
            }
            else
            {
                return Capital.primary;
            }
        }
        private class DataGen
        {
            public Bus Bus { get; set; }
            public List<WayPointsSchedule> WayPointsSchedules { get; set; }
            public List<Coords> Coords { get; set; }
            public List<Path> Paths { get; set; }
            public List<Schedule> Schedules { get; set; }
            public List<Bus> ClonnedBuses { get; set; }
            public DataGen()
            {
                WayPointsSchedules = new List<WayPointsSchedule>();
                Coords = new List<Coords>();
                Paths = new List<Path>();
                Schedules = new List<Schedule>();
            }
        }



    }
}
