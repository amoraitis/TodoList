using TodoList.Core.Models;
using System.Collections.Generic;

namespace TodoList.Web.Models
{
    public class ManageUsersViewModel
    {
        public IEnumerable<ApplicationUser> Administrators { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}
