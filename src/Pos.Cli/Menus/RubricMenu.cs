using Pos.Application.Facades;
using Pos.Cli.Support;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Cli.Menus;

public sealed class RubricMenu
{
    private readonly PosFacade _facade;
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;

    public RubricMenu(
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
            _output.WriteLine("========== РУБРИКАТОРЫ ==========");
            _output.WriteLine("1. Показать все рубрикаторы");
            _output.WriteLine("2. Создать рубрикатор");
            _output.WriteLine("3. Добавить критерий");
            _output.WriteLine("4. Просмотреть критерии");
            _output.WriteLine("5. Удалить рубрикатор");
            _output.WriteLine("6. Удалить критерий");
            _output.WriteLine();
            _output.WriteLine("0. Назад");

            var choice = _input.ReadMenuChoice(
                "Выберите действие: ",
                6);

            if (choice is null || choice == 0)
                return;

            switch (choice)
            {
                case 1:
                    ExecuteSafely(ListRubrics);
                    break;
                case 2:
                    CreateRubric();
                    break;
                case 3:
                    AddCriterion();
                    break;
                case 4:
                    ViewCriteria();
                    break;
                case 5:
                    DeleteRubric();
                    break;
                case 6:
                    DeleteCriterion();
                    break;
            }
        }
    }

    private void ListRubrics()
    {
        var rubrics = GetSortedRubrics();

        if (rubrics.Length == 0)
        {
            _output.WriteLine("Рубрикаторы не найдены.");
            return;
        }

        PrintRubrics(rubrics);
    }

    private void CreateRubric()
    {
        ExecuteSafely(() =>
        {
            var rubric = _facade.CreateRubric();

            _output.WriteLine("Рубрикатор успешно создан.");
            _output.WriteLine($"ID: {rubric.Id}");
        });
    }

    private void AddCriterion()
    {
        var rubrics = GetSortedRubrics();

        if (rubrics.Length == 0)
        {
            _output.WriteLine(
                "Сначала необходимо создать рубрикатор.");
            return;
        }

        PrintRubrics(rubrics);

        var selection = _input.ReadSelectionOrCancel(
            "Выберите рубрикатор (0 — отмена): ",
            rubrics.Length);

        if (selection is null)
            return;

        var rubric = rubrics[selection.Value - 1];

        var name = _input.ReadRequiredText(
            "Название критерия (0 — отмена): ",
            "0");

        if (name is null)
            return;

        var maxScore = _input.ReadDecimalOrCancel(
            "Максимальный балл (> 0, 0 — отмена): ",
            0m,
            minimumInclusive: false);

        if (maxScore is null)
            return;

        var weight = _input.ReadDecimalOrCancel(
            "Вес критерия (> 0 и <= 1, 0 — отмена): ",
            0m,
            1m,
            minimumInclusive: false);

        if (weight is null)
            return;

        ExecuteSafely(() =>
        {
            var criterion = _facade.AddCriterion(
                rubric.Id,
                name,
                new Score(maxScore.Value),
                new Weight(weight.Value));

            _output.WriteLine();
            _output.WriteLine("Критерий успешно добавлен.");
            _output.WriteLine($"Название: {criterion.Name}");
            _output.WriteLine(
                $"Максимальный балл: {criterion.MaxScore.Value}");
            _output.WriteLine($"Вес: {criterion.Weight.Value}");
        });
    }

    private void ViewCriteria()
    {
        var rubric = SelectRubric();

        if (rubric is null)
            return;

        var criteria = _facade.GetCriteria(rubric.Id);

        _output.WriteLine();
        _output.WriteLine("========== КРИТЕРИИ ==========");
        _output.WriteLine($"Рубрикатор: {rubric.Id}");

        if (criteria.Count == 0)
        {
            _output.WriteLine(
                "В этом рубрикаторе пока нет критериев.");
            return;
        }

        PrintCriteria(criteria);

        var totalWeight = criteria.Sum(
            criterion => criterion.Weight.Value);

        _output.WriteLine($"Суммарный вес: {totalWeight}");
    }

    private void DeleteRubric()
    {
        var rubric = SelectRubric();

        if (rubric is null)
            return;

        var confirmed = ConfirmDeletion(
            $"Удалить рубрикатор {rubric.Id}?");

        if (confirmed != true)
            return;

        ExecuteSafely(() =>
        {
            _facade.DeleteRubric(rubric.Id);
            _output.WriteLine("Рубрикатор успешно удалён.");
        });
    }

    private void DeleteCriterion()
    {
        var rubric = SelectRubric();

        if (rubric is null)
            return;

        var criteria = _facade.GetCriteria(rubric.Id)
            .ToArray();

        if (criteria.Length == 0)
        {
            _output.WriteLine(
                "В этом рубрикаторе нет критериев.");
            return;
        }

        _output.WriteLine();
        _output.WriteLine("========== КРИТЕРИИ ==========");
        PrintCriteria(criteria);

        var selection = _input.ReadSelectionOrCancel(
            "Выберите критерий для удаления (0 — отмена): ",
            criteria.Length);

        if (selection is null)
            return;

        var criterion = criteria[selection.Value - 1];

        var confirmed = ConfirmDeletion(
            $"Удалить критерий «{criterion.Name}»?");

        if (confirmed != true)
            return;

        ExecuteSafely(() =>
        {
            _facade.DeleteCriterion(
                rubric.Id,
                criterion.Id);

            _output.WriteLine("Критерий успешно удалён.");
        });
    }

    private Rubric? SelectRubric()
    {
        var rubrics = GetSortedRubrics();

        if (rubrics.Length == 0)
        {
            _output.WriteLine("Рубрикаторы не найдены.");
            return null;
        }

        PrintRubrics(rubrics);

        var selection = _input.ReadSelectionOrCancel(
            "Выберите рубрикатор (0 — отмена): ",
            rubrics.Length);

        if (selection is null)
            return null;

        return rubrics[selection.Value - 1];
    }

    private bool? ConfirmDeletion(string prompt)
    {
        while (true)
        {
            var answer = _input.ReadRequiredText(
                $"{prompt} (д/н, 0 — отмена): ",
                "0");

            if (answer is null)
            {
                _output.WriteLine("Удаление отменено.");
                return null;
            }

            switch (answer.ToLowerInvariant())
            {
                case "д":
                case "да":
                case "y":
                case "yes":
                    return true;

                case "н":
                case "нет":
                case "n":
                case "no":
                    _output.WriteLine("Удаление отменено.");
                    return false;

                default:
                    _output.WriteLine(
                        "Введите «д», «н» или «0» для отмены.");
                    break;
            }
        }
    }

    private Rubric[] GetSortedRubrics()
    {
        return _facade.GetRubrics()
            .OrderBy(rubric => rubric.Id)
            .ToArray();
    }

    private void PrintRubrics(Rubric[] rubrics)
    {
        _output.WriteLine();
        _output.WriteLine(
            "========== СПИСОК РУБРИКАТОРОВ ==========");

        for (var i = 0; i < rubrics.Length; i++)
        {
            var rubric = rubrics[i];

            _output.WriteLine(
                $"{i + 1}. ID: {rubric.Id} | " +
                $"Критериев: {rubric.Criteria.Count}");
        }
    }

    private void PrintCriteria(
        IReadOnlyCollection<Criterion> criteria)
    {
        _output.WriteLine(
            "№ | Название | Максимальный балл | Вес");

        var index = 1;

        foreach (var criterion in criteria)
        {
            _output.WriteLine(
                $"{index++} | {criterion.Name} | " +
                $"{criterion.MaxScore.Value} | " +
                $"{criterion.Weight.Value}");
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
