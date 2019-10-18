using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Core.Models
{
    public class FileInfo
    {
        [Required, Key]
        public Guid TodoId { get; set; }

        [MaxLength(500)]
        public string Path { get; set; }

        public long Size { get; set; }
    }
}