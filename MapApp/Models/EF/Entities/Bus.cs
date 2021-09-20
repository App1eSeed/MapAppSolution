using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class Bus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Operator { get; set; }
        public List<Schedule> Schedule { get; set; }
        public List<WayPointsSchedule> WayPointsSchedule { get; set; }

        public Bus()
        {
            WayPointsSchedule = new List<WayPointsSchedule>();
            Schedule = new List<Schedule>();
        }
    }
}
