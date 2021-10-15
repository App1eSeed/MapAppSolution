using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string WayPointId { get; set; }
        public WayPointsSchedule WayPointsSchedule { get; set; }
        public DayOfWeek ArrivalDay { get; set; }
        public DayOfWeek DepartDay { get; set; }

    }
}
