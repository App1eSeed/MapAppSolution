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
        public string BusTypeId { get; set; }
        public BusType BusType { get; set; }
        public List<WayPointsSchedule> WayPointsSchedule { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public List<Transportation> Transportations { get; set; }
        public Bus()
        {
            Transportations = new List<Transportation>();
            WayPointsSchedule = new List<WayPointsSchedule>();
        }
    }
}
