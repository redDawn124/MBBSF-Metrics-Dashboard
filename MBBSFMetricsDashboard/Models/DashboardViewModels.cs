namespace MBBSFMetricsDashboard.Models;

public enum DashboardFilterKind
{
    Coursera,
    GoogleForms,
    Cognito
}

public class DashboardViewModel
{
    public bool CanEdit { get; init; }
    public IReadOnlyList<MetricSourceViewModel> Sources { get; init; } = [];
}

public class MetricSourceViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public DashboardFilterKind FilterKind { get; init; }
    public IReadOnlyList<DashboardOptionViewModel> CourseraSortOptions { get; init; } = [];
    public IReadOnlyList<string> MentorOptions { get; init; } = [];
    public IReadOnlyList<DashboardOptionViewModel> CommunicationMethodOptions { get; init; } = [];
    public IReadOnlyList<string> FloridaCountyOptions { get; init; } = [];
    public IReadOnlyList<string> SchoolOptions { get; init; } = [];
    public IReadOnlyList<string> MajorOptions { get; init; } = [];
    public string SummaryText { get; init; } = string.Empty;
    public IReadOnlyList<string> SummaryTrends { get; init; } = [];
    public DashboardTableViewModel Table { get; init; } = new();
    public IReadOnlyList<DashboardChartViewModel> Charts { get; init; } = [];
    public string EmptyMessage { get; init; } = "No data uploaded yet.";
}

public class DashboardTableViewModel
{
    public IReadOnlyList<string> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = [];
    public int? TruncateColumnIndex { get; init; }
}

public class DashboardChartViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = "bar";
    public IReadOnlyList<string> Labels { get; init; } = [];
    public IReadOnlyList<string> SubLabels { get; init; } = [];
    public decimal? MaxValue { get; init; }
    public IReadOnlyList<DashboardSeriesViewModel> Series { get; init; } = [];
}

public class DashboardSeriesViewModel
{
    public string Label { get; init; } = string.Empty;
    public IReadOnlyList<decimal> Values { get; init; } = [];
}

public class DashboardOptionViewModel
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}
