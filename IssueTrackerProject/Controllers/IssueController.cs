using Microsoft.AspNetCore.Mvc;
using IssueTrackerProject.Models;
using IssueTrackerProject.Data;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace IssueTrackerProject.Controllers
{
    public class IssueController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor: Connects the Controller to your MySQL Database
        public IssueController(AppDbContext context)
        {
            _context = context;
        }

        // --- READ: Display all Issues ---
    public async Task<IActionResult> Index()
    {
        var issues = await _context.Issues.ToListAsync();

        // Calculate stats for the summary row
        ViewBag.TotalIssues = issues.Count;
        ViewBag.OpenIssues = issues.Count(i => i.Status == "Open");
        ViewBag.HighPriority = issues.Count(i => i.Priority == "High");
        ViewBag.ResolvedIssues = issues.Count(i => i.Status == "Resolved");

        return View(issues);
    }

        // --- CREATE: Add New Issues ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Issue newIssue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(newIssue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newIssue);
        }

        // --- EDIT: Update Existing Issues ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            return View(issue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Issue issue)
        {
            if (id != issue.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(issue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Issues.Any(e => e.Id == issue.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(issue);
        }

        // --- DELETE: Remove Issues ---
        // GET: Shows a confirmation page (optional but recommended for links)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            return View(issue);
        }

        // POST: The actual deletion happens here
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    [HttpPost]
public async Task<IActionResult> ImportCSV(IFormFile file)
{
    if (file == null || file.Length == 0) return BadRequest("Please upload a valid CSV file.");

    using (var reader = new StreamReader(file.OpenReadStream()))
    using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<Issue>().ToList();
        
        // Ensure every imported issue gets a timestamp
        foreach (var issue in records)
        {
            issue.CreatedAt = DateTime.Now;
            issue.UpdatedAt = DateTime.Now;
            if (string.IsNullOrEmpty(issue.Status)) issue.Status = "Open";
            if (string.IsNullOrEmpty(issue.Priority)) issue.Priority = "Medium";
        }

        _context.Issues.AddRange(records);
        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(Index));
}

    // --- EXPORT: Generate Excel Report ---
    public IActionResult ExportToExcel()
    {
        var issues = _context.Issues.ToList();

        using (var workbook = new XLWorkbook())
    {
        var worksheet = workbook.Worksheets.Add("Issues");
        
        // Headers - Added Priority to column 5
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Title";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Priority"; // New Header
        worksheet.Cell(1, 6).Value = "Created At";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;

        // Data Rows
        for (int i = 0; i < issues.Count; i++)
        {
            worksheet.Cell(i + 2, 1).Value = issues[i].Id;
            worksheet.Cell(i + 2, 2).Value = issues[i].Title;
            worksheet.Cell(i + 2, 3).Value = issues[i].Description;
            worksheet.Cell(i + 2, 4).Value = issues[i].Status;
            worksheet.Cell(i + 2, 5).Value = issues[i].Priority; // New Data Field
            worksheet.Cell(i + 2, 6).Value = issues[i].CreatedAt.ToString();
        }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        "IssuesReport.xlsx"
                    );
                }
            }
        }
    }
}