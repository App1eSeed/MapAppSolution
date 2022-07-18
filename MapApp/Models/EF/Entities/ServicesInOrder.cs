using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapApp.Models.EF.Entities
{
    public class ServicesInOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string OrderId { get; set; }
        public Order Order { get; set; }
        [Required]
        public string ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
