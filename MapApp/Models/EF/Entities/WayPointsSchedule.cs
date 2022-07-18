using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class WayPointsSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string BusId { get; set; }
        public Bus Bus { get; set; }
        [Required]
        public string PathId { get; set; }
        public Path Path { get; set; }
        [Required]
        public int Sequence { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public TimeSpan CityFromDepartTime { get; set; }
        public int CityFromDepartTimeInSec { get; set; }
        [Required]
        public TimeSpan CityToArrivalTime { get; set; }
        public int CityToArrivalTimeInSec { get; set; }
        public List<Schedule> Schedules { get; set; }
        public List<TransportationWaypoint> TransportationWaypoints { get; set; }
        public WayPointsSchedule()
        {
            Schedules = new List<Schedule>();
            TransportationWaypoints = new List<TransportationWaypoint>();
        }
    }
}
