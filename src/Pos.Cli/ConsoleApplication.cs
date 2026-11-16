using Pos.Application.Facades;
using Pos.Cli.Menus;
using Pos.Cli.Support;

namespace Pos.Cli;

public sealed class ConsoleApplication
{
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;
    private readonly StudentMenu _studentMenu;
    private readonly RubricMenu _rubricMenu;

    public ConsoleApplication(
        PosFacade facade,
        TextReader input,
        TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(facade);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        _input = new ConsoleInput(input, output);
        _output = output;

        _studentMenu = new StudentMenu(
            facade,
            _input,
            output);

        _rubricMenu = new RubricMenu(
            facade,
            _input,
            output);
    }

    public void Run()
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("================================");
            _output.WriteLine("     POS — СИСТЕМА ОЦЕНИВАНИЯ");
            _output.WriteLine("================================");
            _output.WriteLine("1. Управление студентами");
            _output.WriteLine("2. Управление рубрикаторами");
            _output.WriteLine("3. Управление оценками");
            _output.WriteLine("4. Отчёты и результаты");
            _output.WriteLine();
            _output.WriteLine("0. Выход");

            var choice = _input.ReadMenuChoice(
                "Выберите действие: ",
                4);

            if (choice is null || choice == 0)
            {
                _output.WriteLine("Завершение работы.");
                return;
            }

            switch (choice)
            {
                case 1:
                    _studentMenu.Run();
                    break;

                case 2:
                    _rubricMenu.Run();
                    break;

                case 3:
                case 4:
                    _output.WriteLine(
                        "Раздел пока не реализован.");
                    break;
            }
        }
    }
}
