using NodaTime;
using NodaTime.Extensions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amoraitis.TodoList.Models
{
    public class TodoItem
    {
        [Required, Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        public bool Done { get; set; }
        [NotMapped]
        public Instant Added { get; set; }
        [NotMapped]
        public Instant DueTo { get; set; }
        public FileInfo File { get; set; }

        [Obsolete("Property only used for EF-serialization purposes")]
        [DataType(DataType.DateTime)]
        [Column("Added")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DateTime AddedDateTime
        {
            get => Added.ToDateTimeUtc();
            set => Added = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToInstant();
        }

        [Obsolete("Property only used for EF-serialization purposes")]
        [DataType(DataType.DateTime)]
        [Column("DueTo")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DateTime DuetoDateTime
        {
            get => DueTo.ToDateTimeUtc();
            set => DueTo = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToInstant();
        }
    }

    public class FileInfo
    {
        [Required, Key]
        public Guid UserId { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
}