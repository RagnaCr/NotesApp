namespace NotesApp.Models
{
    public class NoteDetailsViewModel
    {
        public required string Title { get; set; }
        public required string ContentHtml { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
