using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Models
{
    public class TodoViewModel
    {
        public TodoItem[] Todos { get; set; }
        public TodoItem[] Dones { get; set; }
    }
}
