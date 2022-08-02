using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCPatient.Models
{
    public class Roles
    {

        public Roles()
        {
            RolesList = new List<IdentityRole>();
        }
        public List<IdentityRole> RolesList { get; set; }
        public string RoleName { get; set; }
    }
}
