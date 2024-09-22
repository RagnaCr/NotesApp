using Microsoft.AspNetCore.Identity;

namespace NotesApp.Models
{
    public class Note
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
