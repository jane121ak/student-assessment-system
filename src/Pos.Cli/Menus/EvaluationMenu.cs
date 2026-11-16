using Pos.Application.Facades;
using Pos.Cli.Support;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Cli.Menus;

public sealed class EvaluationMenu
{
    private readonly PosFacade _facade;
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;

    public EvaluationMenu(
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
            _output.WriteLine("========== ОЦЕНКИ ==========");
            _output.WriteLine("1. Показать оценки студента");
            _output.WriteLine("2. Выставить оценку");
            _output.WriteLine("3. Изменить оценку");
            _output.WriteLine("4. Удалить оценку");
            _output.WriteLine();
            _output.WriteLine("0. Назад");

            var choice = _input.ReadMenuChoice(
                "Выберите действие: ",
                4);

            if (choice is null || choice == 0)
                return;

            switch (choice)
            {
                case 1:
                    ExecuteSafely(ListEvaluations);
                    break;
                case 2:
                    AddEvaluation();
                    break;
                case 3:
                    ChangeEvaluation();
                    break;
                case 4:
                    DeleteEvaluation();
                    break;
            }
        }
    }

    private void ListEvaluations()
    {
        var student = SelectStudent();

        if (student is null)
            return;

        var evaluations = GetEvaluationRows(student);

        if (evaluations.Length == 0)
        {
            _output.WriteLine("Оценки не найдены.");
            return;
        }

        PrintEvaluations(student, evaluations);
    }

    private void AddEvaluation()
    {
        var student = SelectStudent();

        if (student is null)
            return;

        var rubrics = GetSortedRubrics();

        if (rubrics.Length == 0)
        {
            _output.WriteLine("Рубрикаторы не найдены.");
            return;
        }

        PrintRubrics(rubrics);

        var rubricSelection = _input.ReadSelectionOrCancel(
            "Выберите рубрикатор (0 — отмена): ",
            rubrics.Length);

        if (rubricSelection is null)
            return;

        var rubric = rubrics[rubricSelection.Value - 1];

        var criteria = _facade.GetCriteria(rubric.Id)
            .OrderBy(
                criterion => criterion.Name,
                StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        if (criteria.Length == 0)
        {
            _output.WriteLine(
                "В выбранном рубрикаторе нет критериев.");
            return;
        }

        PrintCriteria(criteria);

        var criterionSelection = _input.ReadSelectionOrCancel(
            "Выберите критерий (0 — отмена): ",
            criteria.Length);

        if (criterionSelection is null)
            return;

        var criterion = criteria[criterionSelection.Value - 1];

        var score = _input.ReadDecimal(
            $"Введите балл (от 0 до {criterion.MaxScore.Value}): ",
            0m,
            criterion.MaxScore.Value);

        if (score is null)
            return;

        ExecuteSafely(() =>
        {
            var evaluation = _facade.AddEvaluation(
                student.Id,
                rubric.Id,
                criterion.Id,
                new Score(score.Value));

            _output.WriteLine();
            _output.WriteLine("Оценка успешно выставлена.");
            _output.WriteLine($"Студент: {student.FullName}");
            _output.WriteLine($"Критерий: {criterion.Name}");
            _output.WriteLine(
                $"Результат: {evaluation.Score.Value} / {criterion.MaxScore.Value}");
        });
    }

    private void ChangeEvaluation()
    {
        var student = SelectStudent();

        if (student is null)
            return;

        var selected = SelectEvaluation(
            student,
            "Выберите оценку для изменения (0 — отмена): ");

        if (selected is null)
            return;

        var score = _input.ReadDecimal(
            $"Введите новый балл (от 0 до {selected.MaxScore}): ",
            0m,
            selected.MaxScore);

        if (score is null)
            return;

        ExecuteSafely(() =>
        {
            _facade.ChangeEvaluation(
                selected.Evaluation.Id,
                new Score(score.Value));

            _output.WriteLine("Оценка успешно изменена.");
            _output.WriteLine(
                $"Новый результат: {score.Value} / {selected.MaxScore}");
        });
    }

    private void DeleteEvaluation()
    {
        var student = SelectStudent();

        if (student is null)
            return;

        var selected = SelectEvaluation(
            student,
            "Выберите оценку для удаления (0 — отмена): ");

        if (selected is null)
            return;

        var confirmation = _input.ReadConfirmation(
            $"Удалить оценку по критерию «{selected.CriterionName}»? (д/н): ");

        if (confirmation is null)
            return;

        if (!confirmation.Value)
        {
            _output.WriteLine("Удаление отменено.");
            return;
        }

        ExecuteSafely(() =>
        {
            _facade.CancelEvaluation(selected.Evaluation.Id);
            _output.WriteLine("Оценка успешно удалена.");
        });
    }

    private Student? SelectStudent()
    {
        var students = GetSortedStudents();

        if (students.Length == 0)
        {
            _output.WriteLine("Студенты не найдены.");
            return null;
        }

        PrintStudents(students);

        var selection = _input.ReadSelectionOrCancel(
            "Выберите студента (0 — отмена): ",
            students.Length);

        if (selection is null)
            return null;

        return students[selection.Value - 1];
    }

    private EvaluationRow? SelectEvaluation(
        Student student,
        string prompt)
    {
        var evaluations = GetEvaluationRows(student);

        if (evaluations.Length == 0)
        {
            _output.WriteLine("Оценки не найдены.");
            return null;
        }

        PrintEvaluations(student, evaluations);

        var selection = _input.ReadSelectionOrCancel(
            prompt,
            evaluations.Length);

        if (selection is null)
            return null;

        return evaluations[selection.Value - 1];
    }

    private EvaluationRow[] GetEvaluationRows(Student student)
    {
        return _facade.GetEvaluations(student.Id)
            .Select(evaluation =>
            {
                var rubric = _facade.GetRubric(evaluation.RubricId);
                var criterion = rubric.GetCriterion(
                    evaluation.CriterionId);

                return new EvaluationRow(
                    evaluation,
                    rubric.Id,
                    criterion.Name,
                    criterion.MaxScore.Value);
            })
            .OrderBy(row => row.RubricId)
            .ThenBy(
                row => row.CriterionName,
                StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private Student[] GetSortedStudents()
    {
        return _facade.GetStudents()
            .OrderBy(
                student => student.FullName,
                StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private Rubric[] GetSortedRubrics()
    {
        return _facade.GetRubrics()
            .OrderBy(rubric => rubric.Id)
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

    private void PrintRubrics(Rubric[] rubrics)
    {
        _output.WriteLine();
        _output.WriteLine("========== СПИСОК РУБРИКАТОРОВ ==========");

        for (var i = 0; i < rubrics.Length; i++)
        {
            _output.WriteLine(
                $"{i + 1}. ID: {rubrics[i].Id} | " +
                $"Критериев: {rubrics[i].Criteria.Count}");
        }
    }

    private void PrintCriteria(Criterion[] criteria)
    {
        _output.WriteLine();
        _output.WriteLine("========== КРИТЕРИИ ==========");
        _output.WriteLine("№ | Название | Максимальный балл");

        for (var i = 0; i < criteria.Length; i++)
        {
            var criterion = criteria[i];

            _output.WriteLine(
                $"{i + 1} | {criterion.Name} | " +
                $"{criterion.MaxScore.Value}");
        }
    }

    private void PrintEvaluations(
        Student student,
        EvaluationRow[] evaluations)
    {
        _output.WriteLine();
        _output.WriteLine("========== СПИСОК ОЦЕНОК ==========");
        _output.WriteLine($"Студент: {student.FullName}");
        _output.WriteLine("№ | Рубрикатор | Критерий | Балл");

        for (var i = 0; i < evaluations.Length; i++)
        {
            var row = evaluations[i];

            _output.WriteLine(
                $"{i + 1} | {row.RubricId} | " +
                $"{row.CriterionName} | " +
                $"{row.Evaluation.Score.Value} / {row.MaxScore}");
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

    private sealed record EvaluationRow(
        Evaluation Evaluation,
        Guid RubricId,
        string CriterionName,
        decimal MaxScore);
}
