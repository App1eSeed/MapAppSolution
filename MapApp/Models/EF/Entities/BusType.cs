using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class BusType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Bus> Buses { get; set; }
        public BusType()
        {
            Buses = new List<Bus>();
        }
    }
}
