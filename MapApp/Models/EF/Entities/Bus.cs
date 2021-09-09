﻿using System;
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
        public string FromCityId { get; set; }
        public City FromCity { get; set; }
        public string ToCityId { get; set; }
        public City ToCity { get; set; }
        public List<Path> Path { get; set; }
        public List<Schedule> Schedule { get; set; }
        public List<WayPointsSchedule> WayPointsSchedule { get; set; }

        public Bus()
        {
            Path = new List<Path>();
            WayPointsSchedule = new List<WayPointsSchedule>();
            Schedule = new List<Schedule>();
        }
    }
}
