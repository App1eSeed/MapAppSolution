using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class State
    {
        public string Id { get; set; }
        public string CountryId { get; set; }
        public Country Country { get; set; }
        public string Name { get; set; }
        public List<City> Cities { get; set; }

        public State()
        {
            Cities = new List<City>();
        }
    }
}
