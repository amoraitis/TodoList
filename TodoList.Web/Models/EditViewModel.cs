using System;

namespace TodoList.Web.Models
{
    public class EditViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}