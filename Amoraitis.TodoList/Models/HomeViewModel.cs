using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Models
{
    public class HomeViewModel
    {
        public TodoItem[] RecentlyAddedTodos { get; set; }
        public TodoItem[] CloseDueToTodos { get; set; }
    }
}
