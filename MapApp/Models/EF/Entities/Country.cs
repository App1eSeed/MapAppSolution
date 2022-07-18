using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string CountryCodeISO2 { get; set; }
        public string CountryCodeISO3 { get; set; }
        public List<City> Cities { get; set; }
        public List<State> States { get; set; }
        public Country()
        {
            Cities = new List<City>();
            States = new List<State>();
        }
    }
}
