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
        [Required]
        public string WayPointsScheduleId { get; set; }
        public WayPointsSchedule WayPointsSchedule { get; set; }
        [Required]
        public DayOfWeek ArrivalDay { get; set; }
        [Required]
        public DayOfWeek DepartDay { get; set; }

    }
}
