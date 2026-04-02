using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class TodoService
    {
        private List<Todo> todos = new List<Todo>();
        private const string filePath = "todos.json";

        public TodoService()
        {
            LoadFromFile();
        }

        //Crud

        public void AddTodo(Todo todo)
        {
            todos.Add(todo);
            SaveToFile();
        }

        public void MarkAsCompleted(int index)
        {
            if (index >= 0 && index < todos.Count)
            {
                todos[index].MarkAsCompleted();
                SaveToFile();
            }
        }

        public void EditTodo(int index)
        {
            if (index < 0 || index >= todos.Count) return;

            var todo = todos[index];

            Console.Write("New title: ");
            var title = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(title))
                todo.Title = title;

            Console.Write("New due date (yyyy-MM-dd HH:mm): ");
            if (DateTime.TryParse(Console.ReadLine(), out var date))
                todo.DueDate = date;

            Console.Write("New project: ");
            var project = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(project))
                todo.Project = project;

            Console.Write("Priority (Low, Medium, High): ");
            if (Enum.TryParse(Console.ReadLine(), out TodoPriority priority))
                todo.Priority = priority;

            Console.WriteLine("\n✅ Todo updated!");

            SaveToFile(); // ✅ FIXED
        }

        public void DeleteTodo(int index)
        {
            if (index >= 0 && index < todos.Count)
            {
                todos.RemoveAt(index);
                SaveToFile();
            }
        }

        // ========================
        // Read and display
        // ========================

        public void ListTodos()
        {
            Console.Clear();

            if (!todos.Any())
            {
                Console.WriteLine("No todos found.");
                return;
            }

            Console.WriteLine("\n=== TODOS ===\n");

            for (int i = 0; i < todos.Count; i++)
            {
                var todo = todos[i];

                if (todo.IsOverdue)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (todo.Priority == TodoPriority.High)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = todo.Color;

                Console.WriteLine($"[{i + 1}] {todo}");

                Console.ResetColor();
            }
        }

        public void FilterByStatus(TodoStatus status)
        {
            var filtered = todos.Where(t => t.Status == status);

            foreach (var todo in filtered)
                Console.WriteLine(todo);
        }

        public void GroupByProject()
        {
            var groups = todos.GroupBy(t => t.Project);

            foreach (var group in groups)
            {
                Console.WriteLine($"\n=== {group.Key} ===");

                foreach (var todo in group)
                    Console.WriteLine(todo);
            }
        }

        public void SortByDueDate()
        {
            var sorted = todos.OrderBy(t => t.DueDate);

            foreach (var todo in sorted)
                Console.WriteLine(todo);
        }

        public void Search(string keyword)
        {
            var results = todos.Where(t =>
                t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (t.Description != null && t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                t.Project.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            foreach (var todo in results)
                Console.WriteLine(todo);
        }

        // ========================
        // Json persistance
        // ========================

        private void SaveToFile()
        {
            var json = JsonSerializer.Serialize(todos, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        private void LoadFromFile()
        {
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            todos = JsonSerializer.Deserialize<List<Todo>>(json) ?? new List<Todo>();
        }

        public List<Todo> GetTodos()
        {
            return todos;
        }

        public void Save()
        {
            SaveToFile();
        }
    }
}