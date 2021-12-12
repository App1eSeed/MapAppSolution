using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class Path
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string CityFromId { get; set; }
        public City CityFrom { get; set; }
        public string CityToId { get; set; }
        public City CityTo { get; set; }
        public float Distance { get; set; }
        public int Time { get; set; }
        public int CoordsCount { get; set; }
        public List<Coords> Coords { get; set; }
        public List<WayPointsSchedule> WayPointsSchedules { get; set; }
        public Path()
        {
            Coords = new List<Coords>();
            WayPointsSchedules = new List<WayPointsSchedule>();
        }
    }
}
