using System;

namespace IssueTrackerProject.Models
{
    public class Issue
    {
        // Properties
        public int Id { get; set; }
        
        public string? ReportedBy { get; set; } // Stores the Email/Username of the QA
        
        public string? AssignedTo { get; set; } // Stores the Email/Username of the Dev

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string Status { get; set; }
        
        // NEW PROPERTY
        public string Priority { get; set; } 

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Constructor
        public Issue()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Status = "Open";
            
            // Default priority for new issues
            Priority = "Medium"; 
        }
    }
}