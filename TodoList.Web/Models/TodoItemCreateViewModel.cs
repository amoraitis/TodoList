using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Web.Models
{
    public class TodoItemCreateViewModel
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(200)]
        [MinLength(15)]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DuetoDateTime { get; set; }

        [RegularExpression(@"^(?:[a-zA-Z0-9_\-]*,?){0,3}$", ErrorMessage = "Maximum 3 comma separated tags!")]
        public string Tags { get; set; }
    }
}
