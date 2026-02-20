using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // This must be exactly like this
using Microsoft.EntityFrameworkCore;
using IssueTrackerProject.Models;

namespace IssueTrackerProject.Data
{
    public class AppDbContext : IdentityDbContext // This will now be recognized
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Issue> Issues { get; set; } = default!;
    }
}