using Microsoft.AspNetCore.Mvc;
using IssueTrackerProject.Models;
using IssueTrackerProject.Data;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IssueTrackerProject.Controllers
{
    [Authorize] // Locks dashboard to logged-in users
    public class IssueController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IssueController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- READ: Dashboard & Analytics ---
        public async Task<IActionResult> Index()
        {
            var issues = await _context.Issues.ToListAsync();

            // Stats for summary cards and chart
            ViewBag.TotalIssues = issues.Count;
            ViewBag.OpenIssues = issues.Count(i => i.Status == "Open");
            ViewBag.ResolvedIssues = issues.Count(i => i.Status == "Resolved");
            ViewBag.HighPriority = issues.Count(i => i.Priority == "High");
            ViewBag.MediumCount = issues.Count(i => i.Priority == "Medium");
            ViewBag.LowCount = issues.Count(i => i.Priority == "Low");

            return View(issues);
        }

        // --- CREATE: QA and Admin Only ---
        [Authorize(Roles = "QA Tester, Admin")]
        public async Task<IActionResult> Create()
        {
            var developers = await _userManager.GetUsersInRoleAsync("Backend Dev");
            ViewBag.DeveloperList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(developers, "UserName", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QA Tester, Admin")]
        public async Task<IActionResult> Create(Issue newIssue)
        {
            if (ModelState.IsValid)
            {
                newIssue.ReportedBy = User.Identity?.Name; // Auto-track reporter
                newIssue.CreatedAt = DateTime.Now;
                newIssue.UpdatedAt = DateTime.Now;

                _context.Add(newIssue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Issue logged successfully!";
                return RedirectToAction(nameof(Index));
            }
            // Reload list if validation fails
            var developers = await _userManager.GetUsersInRoleAsync("Backend Dev");
            ViewBag.DeveloperList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(developers, "UserName", "UserName");
            return View(newIssue);
        }

        // --- EDIT: For all authenticated users ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();
            
            var developers = await _userManager.GetUsersInRoleAsync("Backend Dev");
            ViewBag.DeveloperList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(developers, "UserName", "UserName");

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
                    issue.UpdatedAt = DateTime.Now;
                    _context.Update(issue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Issue #{id} updated!";
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

        // --- DELETE: Admin Only ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Issue deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- IMPORT CSV ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportCSV(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(Index));

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Issue>().ToList();
                foreach (var issue in records)
                {
                    issue.CreatedAt = DateTime.Now;
                    issue.ReportedBy = User.Identity?.Name; // Accountability
                }
                _context.Issues.AddRange(records);
                await _context.SaveChangesAsync();
                TempData["Success"] = "CSV Data Imported!";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- EXPORT EXCEL ---
        public IActionResult ExportToExcel()
        {
            var issues = _context.Issues.ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Issues Report");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Title";
                worksheet.Cell(1, 3).Value = "Status";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Reported By";
                worksheet.Cell(1, 6).Value = "Assigned To";

                for (int i = 0; i < issues.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = issues[i].Id;
                    worksheet.Cell(i + 2, 2).Value = issues[i].Title;
                    worksheet.Cell(i + 2, 3).Value = issues[i].Status;
                    worksheet.Cell(i + 2, 4).Value = issues[i].Priority;
                    worksheet.Cell(i + 2, 5).Value = issues[i].ReportedBy;
                    worksheet.Cell(i + 2, 6).Value = issues[i].AssignedTo;
                }
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }
            }
        }
    }
}