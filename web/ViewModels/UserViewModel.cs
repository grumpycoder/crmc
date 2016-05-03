using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace web.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        //public bool EmailConfirmed { get; set; }
        //public int Level { get; set; }
        //public DateTime JoinDate { get; set; }
        public string[] Roles { get; set; }

        //public IList<Claim> Claims { get; set; }
    }
}