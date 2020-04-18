using System;

namespace TodoList.Web.Models
{
    public class CalendarTodoViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int DueToDay { get; set; }
    }
}