using System;
using System.Linq;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new TodoService();
            bool running = true;

            string[] menuOptions =
            {
                "Add Todo",
                "List Todos",
                "Complete Todo",
                "Filter by Status",
                "Group by Project",
                "Edit Todo",
                "Delete Todo",
                "Sort by Due Date",
                "Search",
                "Exit"
            };

            while (running)
            {
                int choice = ShowMenu(menuOptions);

                Console.Clear();

                switch (choice)
                {
                    case 0: AddTodo(service); break;
                    case 1: InteractiveList(service); break;
                    case 2: CompleteTodo(service); break;
                    case 3: Filter(service); break;
                    case 4: service.GroupByProject(); break;
                    case 5: Edit(service); break;
                    case 6: Delete(service); break;
                    case 7: ShowSortedTodos(service); break;
                    case 8: Search(service); break;
                    case 9: running = false; break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        static void AddTodo(TodoService service)
        {
            Console.Clear();
            WriteCentered("=== ADD TODO ===\n");

            Console.Write("Title: ");
            var title = Console.ReadLine() ?? "";

            Console.Write("Due Date (yyyy-MM-dd HH:mm): ");

            DateTime dueDate;
            while (!DateTime.TryParseExact(
                Console.ReadLine(),
                "yyyy-MM-dd HH:mm",
                null,
                System.Globalization.DateTimeStyles.None,
                out dueDate))
            {
                Console.Write("❌ Invalid format. Use yyyy-MM-dd HH:mm: ");
            }

            Console.Write("Project: ");
            var project = Console.ReadLine() ?? "";

            var todo = new Todo(title, dueDate, project);
            service.AddTodo(todo);
            service.Save(); // ✅ FIX

            Console.WriteLine("\n✅ Todo added!");
        }

        static void CompleteTodo(TodoService service)
        {
            int index = SelectTodo(service, "Complete Todo");
            if (index == -1) return;

            service.MarkAsCompleted(index);
            service.Save(); // ✅ FIX

            Console.WriteLine("\n✅ Todo marked as completed!");
        }

        static void Edit(TodoService service)
        {
            int index = SelectTodo(service, "Edit Todo");
            if (index == -1) return;

            var todo = service.GetTodos()[index];

            Console.Clear();
            WriteCentered("=== EDIT TODO ===\n");

            Console.Write($"Title ({todo.Title}): ");
            var title = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(title))
                todo.Title = title;

            Console.Write($"Due Date ({todo.DueDate:yyyy-MM-dd HH:mm}): ");
            var dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput) &&
                DateTime.TryParseExact(dateInput, "yyyy-MM-dd HH:mm", null,
                    System.Globalization.DateTimeStyles.None, out DateTime newDate))
            {
                todo.DueDate = newDate;
            }

            Console.Write($"Project ({todo.Project}): ");
            var project = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(project))
                todo.Project = project;

            string[] statusOptions = { "Pending", "InProgress", "Completed" };
            int statusChoice = ShowSimpleMenu("Change Status", statusOptions);

            if (statusChoice != -1)
            {
                todo.Status = (TodoStatus)statusChoice;
            }

            service.Save(); // ✅ FIX
            Console.WriteLine("\n✅ Todo updated!");
        }

        static void Delete(TodoService service)
        {
            int index = SelectTodo(service, "Delete Todo");
            if (index == -1) return;

            Console.Write("\nAre you sure? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() == "y")
            {
                service.DeleteTodo(index);
                service.Save(); // ✅ FIX
                Console.WriteLine("🗑 Deleted!");
            }
        }

        static void Search(TodoService service)
        {
            Console.Clear();
            WriteCentered("=== SEARCH ===\n");

            Console.Write("Keyword: ");
            string keyword = Console.ReadLine() ?? "";

            var results = service.GetTodos()
                .Where(t =>
                    t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    t.Project.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.Clear();

            if (!results.Any())
            {
                WriteCentered("No results.");
            }
            else
            {
                DrawBox(results.Select((t, i) => $"{i + 1}. {t}").ToArray());
            }
        }

        // ========================
        // List
        // ========================

        static void InteractiveList(TodoService service)
        {
            var todos = service.GetTodos();
            if (!todos.Any())
            {
                Console.WriteLine("No todos.");
                Console.ReadKey();
                return;
            }

            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                WriteCentered("=== TODOS ===\n");

                for (int i = 0; i < todos.Count; i++)
                {
                    var todo = todos[i];

                    Console.ForegroundColor =
                        i == selectedIndex ? ConsoleColor.Cyan :
                        todo.IsOverdue ? ConsoleColor.Red :
                        todo.Status == TodoStatus.Completed ? ConsoleColor.DarkGray :
                        ConsoleColor.White;

                    Console.WriteLine($"{(i == selectedIndex ? "> " : "  ")}{i + 1}. {todo}");
                    Console.ResetColor();
                }

                Console.WriteLine("\nEnter=Toggle | E=Edit | D=Delete | ESC=Back");

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                    selectedIndex = (selectedIndex - 1 + todos.Count) % todos.Count;

                else if (key == ConsoleKey.DownArrow)
                    selectedIndex = (selectedIndex + 1) % todos.Count;

                else if (key == ConsoleKey.Enter)
                {
                    var t = todos[selectedIndex];
                    t.Status = t.Status == TodoStatus.Completed ? TodoStatus.Pending : TodoStatus.Completed;
                    service.Save(); // ✅ FIX
                }
                else if (key == ConsoleKey.E)
                {
                    Edit(service);
                }
                else if (key == ConsoleKey.D)
                {
                    service.DeleteTodo(selectedIndex);
                    service.Save(); // ✅ FIX
                    break;
                }
                else if (key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }

        // ========================
        // UI
        // ========================

        static int ShowMenu(string[] options)
        {
            int index = 0;

            while (true)
            {
                Console.Clear();
                DrawLogo();

                DrawBox(options.Select((o, i) =>
                    i == index ? $"> {o} <" : $"  {o}").ToArray());

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                    index = (index - 1 + options.Length) % options.Length;

                else if (key == ConsoleKey.DownArrow)
                    index = (index + 1) % options.Length;

                else if (key == ConsoleKey.Enter)
                    return index;
            }
        }

        static void DrawLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteCentered("==== TODO APP ====");
            Console.ResetColor();
        }

        static void DrawBox(string[] lines)
        {
            int width = lines.Max(l => l.Length) + 4;
            WriteCentered("+" + new string('-', width - 2) + "+");

            foreach (var line in lines)
                WriteCentered($"| {line.PadRight(width - 4)} |");

            WriteCentered("+" + new string('-', width - 2) + "+");
        }

        static void WriteCentered(string text)
        {
            int left = Math.Max((Console.WindowWidth - text.Length) / 2, 0);
            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine(text);
        }

        static int SelectTodo(TodoService service, string title)
        {
            var todos = service.GetTodos();
            if (!todos.Any()) return -1;

            int index = 0;

            while (true)
            {
                Console.Clear();
                WriteCentered(title);

                DrawBox(todos.Select((t, i) =>
                    i == index ? $"> {t}" : $"  {t}").ToArray());

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                    index = (index - 1 + todos.Count) % todos.Count;

                else if (key == ConsoleKey.DownArrow)
                    index = (index + 1) % todos.Count;

                else if (key == ConsoleKey.Enter)
                    return index;

                else if (key == ConsoleKey.Escape)
                    return -1;
            }
        }

        static int ShowSimpleMenu(string title, string[] options)
        {
            return ShowMenu(options);
        }

        static void Filter(TodoService service)
        {
            string[] options = { "Pending", "InProgress", "Completed" };
            int choice = ShowMenu(options);
            service.FilterByStatus((TodoStatus)choice);
        }

        static void ShowSortedTodos(TodoService service)
        {
            var todos = service.GetTodos().OrderBy(t => t.DueDate).ToList();
            DrawBox(todos.Select((t, i) => $"{i + 1}. {t}").ToArray());
        }
    }
}