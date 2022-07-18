using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp.Models.EF.Entities
{
    public enum Capital
    {
        primary,
        admin,
        minor,
        none
    }
    public class City
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }        
        public string? StateId { get; set; }
        public State? State { get; set; }
        [Required]
        public string CountryId { get; set; }
        public Country Country { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public float Longtitude { get; set; }
        [Required]
        public float Latitude { get; set; }
        public int Population { get; set; }
        public Capital Capital { get; set; }
        public List<Path> CityFromPath { get; set; }
        public List<Path> CityToPath { get; set; }
        public City()
        {
            Capital = Capital.admin;
           
            Population = 0;
        }
    }
}
