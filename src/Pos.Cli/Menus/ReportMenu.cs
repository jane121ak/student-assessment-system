using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Pos.Application.Facades;
using Pos.Application.Reports;
using Pos.Cli.Support;
using Pos.Domain.Entities;

namespace Pos.Cli.Menus;

public sealed class ReportMenu
{
    private readonly PosFacade _facade;
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;

    public ReportMenu(
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
            _output.WriteLine("========== ОТЧЁТЫ И РЕЗУЛЬТАТЫ ==========");
            _output.WriteLine("1. Просмотреть отчёт студента");
            _output.WriteLine("2. Экспортировать отчёт");
            _output.WriteLine();
            _output.WriteLine("0. Назад");

            var choice = _input.ReadMenuChoice(
                "Выберите действие: ",
                2);

            if (choice is null || choice == 0)
                return;

            switch (choice)
            {
                case 1:
                    ExecuteSafely(ShowReport);
                    break;

                case 2:
                    ExecuteSafely(ExportReport);
                    break;
            }
        }
    }

    private void ShowReport()
    {
        if (!TrySelectAssessment(out var student, out var rubric))
            return;

        var report = _facade.GetGradeReport(
            student.Id,
            rubric.Id);

        PrintReport(report);
    }

    private void ExportReport()
    {
        if (!TrySelectAssessment(out var student, out var rubric))
            return;

        _output.WriteLine();
        _output.WriteLine("========== ФОРМАТ ЭКСПОРТА ==========");
        _output.WriteLine("1. CSV");
        _output.WriteLine("2. JSON");
        _output.WriteLine("0. Отмена");

        var formatChoice = _input.ReadMenuChoice(
            "Выберите формат: ",
            2);

        if (formatChoice is null || formatChoice == 0)
            return;

        var format = formatChoice.Value == 1
            ? "csv"
            : "json";

        var destinationPath = _input.ReadRequiredText(
            $"Путь к файлу .{format} (0 — отмена): ",
            "0");

        if (destinationPath is null)
            return;

        _facade.ExportGradeReport(
            student.Id,
            rubric.Id,
            format,
            destinationPath);

        _output.WriteLine();
        _output.WriteLine("Отчёт успешно экспортирован.");
        _output.WriteLine($"Файл: {destinationPath}");
    }

    private bool TrySelectAssessment(
        out Student student,
        out Rubric rubric)
    {
        student = null!;
        rubric = null!;

        var students = _facade.GetStudents()
            .OrderBy(
                item => item.FullName,
                StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        if (students.Length == 0)
        {
            _output.WriteLine(
                "Студенты не найдены. Сначала добавьте студента.");
            return false;
        }

        var rubrics = _facade.GetRubrics()
            .OrderBy(item => item.Id)
            .ToArray();

        if (rubrics.Length == 0)
        {
            _output.WriteLine(
                "Рубрикаторы не найдены. Сначала создайте рубрикатор.");
            return false;
        }

        PrintStudents(students);

        var studentSelection = _input.ReadSelectionOrCancel(
            "Выберите студента (0 — отмена): ",
            students.Length);

        if (studentSelection is null)
            return false;

        student = students[studentSelection.Value - 1];

        PrintRubrics(rubrics);

        var rubricSelection = _input.ReadSelectionOrCancel(
            "Выберите рубрикатор (0 — отмена): ",
            rubrics.Length);

        if (rubricSelection is null)
            return false;

        rubric = rubrics[rubricSelection.Value - 1];

        return true;
    }

    private void PrintStudents(Student[] students)
    {
        _output.WriteLine();
        _output.WriteLine("========== ВЫБОР СТУДЕНТА ==========");

        for (var i = 0; i < students.Length; i++)
        {
            var student = students[i];

            _output.WriteLine(
                $"{i + 1}. {student.FullName} | ID: {student.Id}");
        }
    }

    private void PrintRubrics(Rubric[] rubrics)
    {
        _output.WriteLine();
        _output.WriteLine("========== ВЫБОР РУБРИКАТОРА ==========");

        for (var i = 0; i < rubrics.Length; i++)
        {
            var rubric = rubrics[i];

            var criteriaDescription = rubric.Criteria.Count == 0
                ? "критерии не добавлены"
                : string.Join(
                    ", ",
                    rubric.Criteria.Select(criterion => criterion.Name));

            _output.WriteLine(
                $"{i + 1}. ID: {rubric.Id} | Критерии: {criteriaDescription}");
        }
    }

    private void PrintReport(GradeReport report)
    {
        var culture = CultureInfo.CurrentCulture;

        _output.WriteLine();
        _output.WriteLine("========== ОТЧЁТ О РЕЗУЛЬТАТАХ ==========");
        _output.WriteLine($"Студент: {report.StudentFullName}");
        _output.WriteLine($"ID студента: {report.StudentId}");
        _output.WriteLine($"ID рубрикатора: {report.RubricId}");
        _output.WriteLine();

        if (report.Criteria.Count == 0)
        {
            _output.WriteLine(
                "В выбранном рубрикаторе пока нет критериев.");
        }
        else
        {
            _output.WriteLine("Критерий | Балл | Вес");

            foreach (var criterion in report.Criteria)
            {
                var score = criterion.Score.HasValue
                    ? $"{FormatDecimal(criterion.Score.Value)} / {FormatDecimal(criterion.MaxScore)}"
                    : $"— / {FormatDecimal(criterion.MaxScore)}";

                _output.WriteLine(
                    $"{criterion.CriterionName} | {score} | {criterion.Weight.ToString("P2", culture)}");
            }
        }

        _output.WriteLine();

        if (!report.FinalGrade.HasValue)
        {
            _output.WriteLine(
                "Итог пока не рассчитан: ещё нет выставленных оценок.");
            return;
        }

        _output.WriteLine(
            $"Итог по выставленным оценкам: {report.FinalGrade.Value.ToString("P2", culture)}");

        _output.WriteLine(
            $"Нормированное значение: {report.FinalGrade.Value.ToString("0.####", culture)} / 1");
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.CurrentCulture);
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
                or IOException
                or NotSupportedException
                or UnauthorizedAccessException)
        {
            _output.WriteLine($"Ошибка: {exception.Message}");
        }
    }
}
