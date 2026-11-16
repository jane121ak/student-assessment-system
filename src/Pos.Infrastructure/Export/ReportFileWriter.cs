using System.Text;

namespace Pos.Infrastructure.Export;

internal static class ReportFileWriter
{
    public static void Write(string destinationPath, string content)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException(
                "Destination path cannot be empty.",
                nameof(destinationPath));

        ArgumentNullException.ThrowIfNull(content);

        var fullPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);

        var temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(
                temporaryPath,
                content,
                new UTF8Encoding(false));

            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
