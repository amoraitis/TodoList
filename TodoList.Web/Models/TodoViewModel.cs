using TodoList.Core.Models;
using System.Collections.Generic;

namespace TodoList.Web.Models
{
    public class TodoViewModel
    {
        public IEnumerable<TodoItem> Todos { get; set; }
        public IEnumerable<TodoItem> Dones { get; set; }
    }
}
