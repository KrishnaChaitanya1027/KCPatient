using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCPatient.Models
{
    public class UserRole
    {
        public UserRole()
        {
            UserInRole = new List<IdentityUser>();
        }

        public string UserName { get; set; }

        public string RoleId { get; set; }

        public List<IdentityUser> UserInRole { get; set; }
    }
}
