using System;

namespace IssueTrackerProject.Models
{
    public class Issue
    {
        // Properties
        public int Id { get; set; }

        // Adding = string.Empty; fixes the CS8618 warnings
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Constructor
        public Issue()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Status = "Open";
        }
    }
}