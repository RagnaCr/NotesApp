using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Services.Interfaces;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace NotesApp.Services
{
    public class NotesService : INotesService
    {
        private readonly ApplicationDbContext _context;

        public NotesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetUserNotesAsync(string? userId)
        {
            return await _context.Notes.Where(n => n.UserId == userId).ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int id, string? userId)
        {
            return await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task CreateNoteAsync(Note note, string? userId)
        {
            note.UserId = userId;
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            _context.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            note.UpdatedAt = DateTime.Now;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int id, string? userId)
        {
            var note = await GetNoteByIdAsync(id, userId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSelectedNotesAsync(int[] selectedNotes, string? userId)
        {
            var notesToDelete = await _context.Notes
                .Where(n => selectedNotes.Contains(n.Id) && n.UserId == userId)
                .ToListAsync();

            if (notesToDelete.Count > 0)
            {
                _context.Notes.RemoveRange(notesToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<NoteDetailsViewModel?> GetNoteDetailsAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return null;

            var markdown = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var contentHtml = Markdown.ToHtml(note.Content, markdown);

            return new NoteDetailsViewModel
            {
                Title = note.Title,
                ContentHtml = contentHtml,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
        public bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
        public async Task<FileResult> ExportNotesAsync(int[] selectedNotes, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Проблема с авторизацией.");
            }

            IEnumerable<Note> notes;
            if (selectedNotes != null && selectedNotes.Length > 0)
            {
                notes = await _context.Notes
                    .Where(n => n.UserId == userId && selectedNotes.Contains(n.Id))
                    .ToListAsync();
            }
            else
            {
                notes = await _context.Notes
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
            }

            if (!notes.Any())
            {
                throw new InvalidOperationException("Нет заметок для экспорта.");
            }

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var note in notes)
                {
                    var sanitizedTitle = string.Concat(note.Title.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
                    var entryName = $"{sanitizedTitle}.md";

                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        await writer.WriteAsync(note.Content);
                    }
                }
            }
            memoryStream.Position = 0; // Сбросить позицию потока перед возвратом

            return new FileContentResult(memoryStream.ToArray(), "application/zip")
            {
                FileDownloadName = "Notes.zip"
            };
        }
        public async Task<string> ImportNotesAsync(IFormFile file, string? userId)
        {
            if (file == null || file.Length == 0){
                return "Ошибка: Файл не выбран.";
            }

            if (string.IsNullOrEmpty(userId)){
                return "Ошибка: Проблема с авторизацией.";
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".md")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var content = await reader.ReadToEndAsync();
                        var title = Path.GetFileNameWithoutExtension(file.FileName);

                        title = Regex.Replace(title, @"[<>:""/\\|?*]", "");

                        var note = new Note
                        {
                            Title = title,
                            Content = content,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Notes.Add(note);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (fileExtension == ".zip")
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;

                        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (entry.Name.EndsWith(".md"))
                                {
                                    using (var reader = new StreamReader(entry.Open()))
                                    {
                                        var content = await reader.ReadToEndAsync();
                                        var title = Path.GetFileNameWithoutExtension(entry.Name);

                                        title = Regex.Replace(title, @"[<>:""/\\|?*]", "");

                                        var note = new Note
                                        {
                                            Title = title,
                                            Content = content,
                                            UserId = userId,
                                            CreatedAt = DateTime.UtcNow,
                                            UpdatedAt = DateTime.UtcNow
                                        };

                                        _context.Notes.Add(note);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    return "Ошибка: Недопустимый формат файла. Пожалуйста, загрузите .md или .zip файл.";
                }

                return "Заметки успешно импортированы.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при импорте заметок: {ex.Message}";
            }
        }
    }
}
