using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class State
    {
        public string Id { get; set; }
        [Required]
        public string CountryId { get; set; }
        public Country Country { get; set; }
        [Required]
        public string Name { get; set; }
        public List<City> Cities { get; set; }

        public State()
        {
            Cities = new List<City>();
        }
    }
}
