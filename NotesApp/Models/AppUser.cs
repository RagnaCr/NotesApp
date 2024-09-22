using Microsoft.AspNetCore.Identity;

namespace NotesApp.Models
{
    public class AppUser : IdentityUser
    {
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
