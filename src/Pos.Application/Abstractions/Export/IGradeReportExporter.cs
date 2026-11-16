using Pos.Application.Reports;

namespace Pos.Application.Abstractions.Export;

public interface IGradeReportExporter
{
    string Format { get; }

    void Export(GradeReport report, string destinationPath);
}
