using System;
using System.Collections.Generic;
using System.IO;
using MyDemoProj.Mapping;
using MyDemoProj.Models;
using MyDemoProj.Validation;

namespace MyDemoProj.App
{
    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            var fileName = "користувачі.csv";

           CreateSampleCsv(fileName);
            Console.WriteLine($"Зразок CSV створено/перезаписано: {fileName}");

            var users = CsvMapper<User>.MapFromFile(fileName, out var mapErrors);

            Console.WriteLine($"Імпортовано {users.Count} записи. Mapper доповів {mapErrors.Count} помилок.");
            foreach (var e in mapErrors) Console.WriteLine("Помилка мапперу: " + e);

            int validCount = 0, invalidCount = 0;
            for (int i = 0; i < users.Count; i++)
            {
                var u = users[i];
                var vErrors = ModelValidator.Validate(u);
                if (vErrors.Count == 0)
                {
                    validCount++;
                    Console.WriteLine($"Запис {i + 1}: ВІРНИЙ -> {u}");
                }
                else
                {
                    invalidCount++;
                    Console.WriteLine($"Запис {i + 1}: НЕВІРНИЙ -> {u}");
                    foreach (var ve in vErrors) Console.WriteLine("  Перевірка: " + ve);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Підсумок: Загалом={users.Count}, Правильних={validCount}, Неправильних={invalidCount}");
        }

        private static void CreateSampleCsv(string path)
        {
            var lines = new List<string>
            {
                "Ім'я Користувача,Єл.Пошта,Вік",
                "alice,alice@example.com,30",         // valid
                ",missing@example.com,25",            // Нема і'мя
                "carol,short@example.com,17",         // Правильно
                "bob,bob@example.com,25",             // Правильно
                "dave,dave@example.com,notanumber",   // Вік не цифра
                ",fcom,сороктри"          // Повністю не правильно
            };
            File.WriteAllLines(path, lines);
        }
    }
}