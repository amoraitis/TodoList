using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Web.Models
{
    public class EditViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        [RegularExpression(@"^(?:[a-zA-Z0-9_\-]*,?){0,3}$", ErrorMessage = "Maximum 3 comma separated tags!")]
        public string Tags { get; set; }
    }
}