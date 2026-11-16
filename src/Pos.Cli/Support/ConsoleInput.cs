using System.Globalization;

namespace Pos.Cli.Support;

public sealed class ConsoleInput
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleInput(TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        _input = input;
        _output = output;
    }

    public string? ReadLine(string prompt)
    {
        _output.Write(prompt);
        return _input.ReadLine();
    }

    public string? ReadRequiredText(
        string prompt,
        string? cancelValue = null)
    {
        while (true)
        {
            var value = ReadLine(prompt);

            if (value is null)
                return null;

            var trimmed = value.Trim();

            if (cancelValue is not null &&
                string.Equals(
                    trimmed,
                    cancelValue,
                    StringComparison.OrdinalIgnoreCase))
            {
                _output.WriteLine("Ввод отменён.");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(trimmed))
                return trimmed;

            _output.WriteLine("Значение не может быть пустым.");
        }
    }


    public decimal? ReadDecimal(
        string prompt,
        decimal minimum,
        decimal? maximum = null,
        bool minimumInclusive = true)
    {
        if (maximum.HasValue && maximum.Value < minimum)
            throw new ArgumentOutOfRangeException(nameof(maximum));

        while (true)
        {
            var value = ReadLine(prompt);

            if (value is null)
                return null;

            var normalized = value.Trim().Replace(',', '.');

            var isValid = decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var result);

            var aboveMinimum = minimumInclusive
                ? result >= minimum
                : result > minimum;

            var belowMaximum = !maximum.HasValue ||
                               result <= maximum.Value;

            if (isValid && aboveMinimum && belowMaximum)
                return result;

            _output.WriteLine(
                "Некорректное число или значение вне допустимого диапазона.");
        }
    }

    public int? ReadSelection(string prompt, int itemCount)
    {
        if (itemCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemCount));

        while (true)
        {
            var value = ReadLine(prompt);

            if (value is null)
                return null;

            if (int.TryParse(
                    value.Trim(),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var selection) &&
                selection >= 1 &&
                selection <= itemCount)
            {
                return selection;
            }

            _output.WriteLine(
                $"Введите номер от 1 до {itemCount}.");
        }
    }

    public int? ReadMenuChoice(string prompt, int maximumChoice)
    {
        if (maximumChoice < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumChoice));

        while (true)
        {
            var value = ReadLine(prompt);

            if (value is null)
                return null;

            if (int.TryParse(
                    value.Trim(),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var choice) &&
                choice >= 0 &&
                choice <= maximumChoice)
            {
                return choice;
            }

            _output.WriteLine(
                $"Введите номер от 0 до {maximumChoice}.");
        }
    }


    public bool? ReadConfirmation(string prompt)
    {
        while (true)
        {
            var value = ReadLine(prompt);

            if (value is null)
                return null;

            switch (value.Trim().ToLowerInvariant())
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
                    return false;

                default:
                    _output.WriteLine(
                        "Введите «д» или «н».");
                    break;
            }
        }
    }
}
