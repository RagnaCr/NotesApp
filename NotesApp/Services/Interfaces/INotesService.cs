using Microsoft.AspNetCore.Mvc;
using NotesApp.Models;

namespace NotesApp.Services.Interfaces
{
    public interface INotesService
    {
        Task<List<Note>> GetUserNotesAsync(string? userId);
        Task<Note?> GetNoteByIdAsync(int id, string? userId);
        Task CreateNoteAsync(Note note, string? userId);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id, string? userId);
        Task DeleteSelectedNotesAsync(int[] selectedNotes, string? userId);
        Task<NoteDetailsViewModel?> GetNoteDetailsAsync(int id);
        bool NoteExists(int id);
        Task<string> ImportNotesAsync(IFormFile file, string? userId);
        Task<FileResult> ExportNotesAsync(int[] selectedNotes, string? userId);
    }
}
