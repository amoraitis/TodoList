using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Models
{
    public class ManageUsersViewModel
    {
        public ApplicationUser[] Administrators { get; set; }
        public ApplicationUser[] Everyone { get; set; }
    }
}
