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
        public string BusId { get; set; }
        public Bus Bus { get; set; }
        public string PathId { get; set; }
        public Path Path { get; set; }
        public int Sequence { get; set; }
        public decimal Price { get; set; }
        public TimeSpan CityFromDepartTime { get; set; }
        public int CityFromDepartTimeInSec { get; set; }
        public TimeSpan CityToArrivalTime { get; set; }
        public int CityToArrivalTimeInSec { get; set; }
        public List<Schedule> Schedules { get; set; }
        public WayPointsSchedule()
        {
            Schedules = new List<Schedule>();
        }
    }
}
