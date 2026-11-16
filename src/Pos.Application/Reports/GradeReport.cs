namespace Pos.Application.Reports;

public sealed record CriterionGradeReport(
    Guid CriterionId,
    string CriterionName,
    decimal MaxScore,
    decimal Weight,
    decimal? Score);

public sealed record GradeReport(
    Guid StudentId,
    string StudentFullName,
    Guid RubricId,
    IReadOnlyCollection<CriterionGradeReport> Criteria,
    decimal? FinalGrade);
