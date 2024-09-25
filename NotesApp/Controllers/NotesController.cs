using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Services.Interfaces;
using System.Security.Claims;

namespace NotesApp.Controllers
{
    [Authorize]
    public class NotesController : BaseController
    {
        private readonly INotesService _notesService;

        public NotesController(INotesService notesService)
        {
            _notesService = notesService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = await _notesService.GetUserNotesAsync(userId);
            return View(notes);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _notesService.CreateNoteAsync(note, userId);
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _notesService.GetNoteByIdAsync(id.Value, userId);
            if (note == null) return NotFound();

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                note.UserId = userId;
                await _notesService.UpdateNoteAsync(note);
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _notesService.GetNoteByIdAsync(id.Value, userId);
            if (note == null) return NotFound();

            return View(note);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notesService.DeleteNoteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedNotes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notesService.DeleteSelectedNotesAsync(selectedNotes, userId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var noteDetails = await _notesService.GetNoteDetailsAsync(id);
            if (noteDetails == null) return NotFound();
            return View(noteDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportNotes(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notesService.ImportNotesAsync(file, userId);

            if (result.Contains("Ошибка"))
            {
                return Content(result);
            }

            return RedirectToAction("Index", "Notes");
        }

        public async Task<IActionResult> ExportNotes(int[] selectedNotes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var fileResult = await _notesService.ExportNotesAsync(selectedNotes, userId);
                return fileResult;
            }
            catch (UnauthorizedAccessException ex)
            {
                return Content(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Content(ex.Message);
            }
            catch (Exception ex)
            {
                return Content($"Произошла ошибка: {ex.Message}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterAction(IFormFile file, string action, int[] selectedNotes)
        {
            switch (action)
            {
                case "DeleteSelected":
                    if (selectedNotes != null && selectedNotes.Length > 0)
                    {
                        return await DeleteSelected(selectedNotes);
                    }
                    break;
                case "ExportNotes":
                        return await ExportNotes(selectedNotes);
                default:
                    if (file != null && file.Length > 0)
                    {
                        await ImportNotes(file);
                    }
                    return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}
