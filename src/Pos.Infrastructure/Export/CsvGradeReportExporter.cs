using System.Globalization;
using System.Text;
using Pos.Application.Abstractions.Export;
using Pos.Application.Reports;

namespace Pos.Infrastructure.Export;

public sealed class CsvGradeReportExporter : IGradeReportExporter
{
    public string Format => "csv";

    public void Export(GradeReport report, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(report.Criteria);

        var builder = new StringBuilder();

        builder.AppendLine(
            "studentId,studentFullName,rubricId,criterionId,criterionName,maxScore,weight,score,finalGrade");

        foreach (var criterion in report.Criteria)
        {
            var fields = new[]
            {
                report.StudentId.ToString(),
                report.StudentFullName,
                report.RubricId.ToString(),
                criterion.CriterionId.ToString(),
                criterion.CriterionName,
                criterion.MaxScore.ToString(CultureInfo.InvariantCulture),
                criterion.Weight.ToString(CultureInfo.InvariantCulture),
                criterion.Score?.ToString(CultureInfo.InvariantCulture) ?? "",
                report.FinalGrade?.ToString(CultureInfo.InvariantCulture) ?? ""
            };

            builder.AppendLine(
                string.Join(",", fields.Select(Escape)));
        }

        ReportFileWriter.Write(destinationPath, builder.ToString());
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') &&
            !value.Contains('"') &&
            !value.Contains('\n') &&
            !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
