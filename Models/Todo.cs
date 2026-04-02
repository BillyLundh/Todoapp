using System;

namespace TodoApp.Models
{
    public class Todo
    {
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public TodoStatus Status { get; set; }
        public TodoPriority Priority { get; set; }
        public string Project { get; set; }
        public string Description { get; set; }
        public TimeSpan EstimatedTime { get; set; }
        public ConsoleColor Color { get; set; }

        public bool IsOverdue =>
            DueDate < DateTime.Now &&
            Status != TodoStatus.Completed;

        public Todo() { }

        public Todo(string title, DateTime dueDate, string project)
        {
            Title = title;
            DueDate = dueDate;
            Project = project;
            Status = TodoStatus.Pending;
            Priority = TodoPriority.Medium;
            Color = ConsoleColor.White;
        }

        public void MarkAsCompleted()
        {
            Status = TodoStatus.Completed;
        }

        public override string ToString()
        {
            string overdueText = IsOverdue ? "⚠ OVERDUE" : "";
            return $"{Title} | Due: {DueDate:yyyy-MM-dd HH:mm} | {Status} | {Priority} | {Project} {overdueText}";
        }
    }
}