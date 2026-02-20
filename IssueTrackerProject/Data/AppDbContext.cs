using Microsoft.EntityFrameworkCore;
using IssueTrackerProject.Models;

namespace IssueTrackerProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        // This represents your table in MySQL
        public DbSet<Issue> Issues { get; set; }
    }
}