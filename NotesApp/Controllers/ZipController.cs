using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Data;
using NotesApp.Models;
using System.IO;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace NotesApp.Controllers
{
    [Authorize]
    public class ZipController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ZipController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ExportNotes()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) { return Content("Проблема с авторизацией."); }

                var notes = await _context.Notes
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
                if (notes.Count == 0) { return Content("Нет заметок для экспорта."); }

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

                // Верните zip-файл для скачивания
                return File(memoryStream.ToArray(), "application/zip", "Notes.zip");

            }
            catch (UnauthorizedAccessException)
            {
                return Content("Ошибка: недостаточно прав для создания архива.");
            }
            catch (IOException ex)
            {
                return Content($"Ошибка ввода-вывода: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Content($"Произошла неожиданная ошибка: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> ImportNotes(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Content("Файл не выбран.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Content("Проблема с авторизацией.");
            }

            try
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

                                    // Удаляем недопустимые символы из заголовка
                                    title = Regex.Replace(title, @"[<>:""/\\|?*]", "");

                                    // Создаем новую заметку
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

                return RedirectToAction("Index", "Notes");
            }
            catch (Exception ex)
            {
                return Content($"Ошибка при импорте заметок: {ex.Message}");
            }
        }


    }
}
