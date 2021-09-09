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
        public int Id { get; set; }
        public string BusId { get; set; }
        public Bus Bus { get; set; }
        public float Longtitude { get; set; }
        public float Latitude { get; set; }
    }
}
