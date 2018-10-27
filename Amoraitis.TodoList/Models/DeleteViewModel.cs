using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Models
{
    public class DeleteViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
    }
}
