using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.API.Models
{
    public class TodoItemDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Title { get; set; }

        [MinLength(15)]
        [MaxLength(200)]
        public string Content { get; set; }

        public bool? Done { get; set; }

        public DateTime DuetoDateTime { get; set; }

        [RegularExpression(@"^(?:[a-zA-Z0-9_\-]*,?){0,3}$", ErrorMessage = "Maximum 3 comma separated tags!")]
        public string Tags { get; set; }
    }
}
