using System.Collections.Generic;

namespace TodoList.Web.Models
{
    public class HomeViewModel
    {
        public IEnumerable<TodoItem> RecentlyAddedTodos { get; set; }
        public IEnumerable<TodoItem> CloseDueToTodos { get; set; }
    }
}
