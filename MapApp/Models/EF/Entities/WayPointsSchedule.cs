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
        public string CityId { get; set; }
        public City City { get; set; }
        public int Sequence { get; set; }
        [DataType(DataType.Time)]
        public DateTime Time { get; set; }
    }
}
