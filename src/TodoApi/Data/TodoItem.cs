using System.ComponentModel.DataAnnotations;
using TodoApi.Data.Enums;

namespace TodoApi.Data
{
    public class TodoItem
    {
        public UserId Id { get; set; }

        [MaxLength(200)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public TodoStatus Status { get; set; }

        public TodoPriority Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public string[]? Tags { get; set; }
    }
}
