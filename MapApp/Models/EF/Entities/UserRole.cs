using System.Collections.Generic;

namespace MapApp.Models.EF.Entities
{
    public class UserRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
        public UserRole()
        {
            Users = new List<User>();
        }
    }
}
