namespace Pos.Tests.Infrastructure.Support;

internal sealed class TemporaryDirectory : IDisposable
{
    public string DirectoryPath { get; }

    public TemporaryDirectory()
    {
        DirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "Pos.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(DirectoryPath);
    }

    public string GetFilePath(string fileName)
    {
        return Path.Combine(DirectoryPath, fileName);
    }

    public void Dispose()
    {
        if (Directory.Exists(DirectoryPath))
            Directory.Delete(DirectoryPath, true);
    }
}
