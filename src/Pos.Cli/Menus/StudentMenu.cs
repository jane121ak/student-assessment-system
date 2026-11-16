using Pos.Application.Facades;
using Pos.Cli.Support;
using Pos.Domain.Entities;

namespace Pos.Cli.Menus;

public sealed class StudentMenu
{
    private readonly PosFacade _facade;
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;

    public StudentMenu(
        PosFacade facade,
        ConsoleInput input,
        TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(facade);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        _facade = facade;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("========== СТУДЕНТЫ ==========");
            _output.WriteLine("1. Показать всех студентов");
            _output.WriteLine("2. Добавить студента");
            _output.WriteLine("3. Удалить студента");
            _output.WriteLine();
            _output.WriteLine("0. Назад");

            var choice = _input.ReadMenuChoice(
                "Выберите действие: ",
                3);

            if (choice is null || choice == 0)
                return;

            switch (choice)
            {
                case 1:
                    ExecuteSafely(ListStudents);
                    break;

                case 2:
                    AddStudent();
                    break;

                case 3:
                    DeleteStudent();
                    break;
            }
        }
    }

    private void ListStudents()
    {
        var students = GetSortedStudents();

        if (students.Length == 0)
        {
            _output.WriteLine("Студенты не найдены.");
            return;
        }

        PrintStudents(students);
    }

    private void AddStudent()
    {
        string? fullName;

        while (true)
        {
            fullName = _input.ReadRequiredText(
                "Введите ФИО студента (0 — отмена): ",
                "0");

            if (fullName is null)
                return;

            if (fullName.Any(char.IsLetter))
                break;

            _output.WriteLine(
                "ФИО должно содержать хотя бы одну букву.");
        }

        ExecuteSafely(() =>
        {
            var student = _facade.AddStudent(fullName);

            _output.WriteLine(
                $"Студент успешно добавлен: {student.FullName}");

            _output.WriteLine($"ID: {student.Id}");
        });
    }

    private void DeleteStudent()
    {
        var students = GetSortedStudents();

        if (students.Length == 0)
        {
            _output.WriteLine("Студенты не найдены.");
            return;
        }

        PrintStudents(students);

        var selection = _input.ReadMenuChoice(
            "Введите номер студента для удаления (0 — отмена): ",
            students.Length);

        if (selection is null || selection == 0)
            return;

        var student = students[selection.Value - 1];

        var confirmation = _input.ReadConfirmation(
            $"Удалить студента {student.FullName}? (д/н): ");

        if (confirmation is null)
            return;

        if (!confirmation.Value)
        {
            _output.WriteLine("Удаление отменено.");
            return;
        }

        ExecuteSafely(() =>
        {
            _facade.DeleteStudent(student.Id);

            _output.WriteLine(
                $"Студент успешно удалён: {student.FullName}");
        });
    }

    private Student[] GetSortedStudents()
    {
        return _facade.GetStudents()
            .OrderBy(
                student => student.FullName,
                StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private void PrintStudents(Student[] students)
    {
        _output.WriteLine();
        _output.WriteLine("========== СПИСОК СТУДЕНТОВ ==========");
        _output.WriteLine("№ | ФИО | ID");

        for (var i = 0; i < students.Length; i++)
        {
            var student = students[i];

            _output.WriteLine(
                $"{i + 1} | {student.FullName} | {student.Id}");
        }
    }

    private void ExecuteSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or InvalidOperationException
                or KeyNotFoundException
                or IOException)
        {
            _output.WriteLine($"Ошибка: {exception.Message}");
        }
    }
}
