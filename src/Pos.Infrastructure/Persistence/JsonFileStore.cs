using System.Text.Json;

namespace Pos.Infrastructure.Persistence;

internal sealed class JsonFileStore<T> where T : class
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public JsonFileStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(
                "File path cannot be empty.",
                nameof(filePath));

        _filePath = Path.GetFullPath(filePath);
    }

    public List<T> ReadAll()
    {
        if (!File.Exists(_filePath))
            return new List<T>();

        try
        {
            return JsonSerializer.Deserialize<List<T>>(
                File.ReadAllText(_filePath),
                _options) ?? new List<T>();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Invalid JSON data in '{_filePath}'.",
                exception);
        }
    }

    public void WriteAll(IReadOnlyCollection<T> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var directory = Path.GetDirectoryName(_filePath)!;

        Directory.CreateDirectory(directory);

        var temporaryPath =
            $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            var json = JsonSerializer.Serialize(records, _options);

            File.WriteAllText(temporaryPath, json);

            File.Move(temporaryPath, _filePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
