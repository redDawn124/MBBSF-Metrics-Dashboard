using MBBSFMetricsDashboard.Models;

namespace MBBSFMetricsDashboard.Services;

public static class DashboardViewModelFactory
{
    private static readonly IReadOnlyList<string> FloridaCounties =
    [
        "Alachua", "Baker", "Bay", "Bradford", "Brevard", "Broward", "Calhoun", "Charlotte", "Citrus", "Clay",
        "Collier", "Columbia", "DeSoto", "Dixie", "Duval", "Escambia", "Flagler", "Franklin", "Gadsden", "Gilchrist",
        "Glades", "Gulf", "Hamilton", "Hardee", "Hendry", "Hernando", "Highlands", "Hillsborough", "Holmes", "Indian River",
        "Jackson", "Jefferson", "Lafayette", "Lake", "Lee", "Leon", "Levy", "Liberty", "Madison", "Manatee",
        "Marion", "Martin", "Miami-Dade", "Monroe", "Nassau", "Okaloosa", "Okeechobee", "Orange", "Osceola", "Palm Beach",
        "Pasco", "Pinellas", "Polk", "Putnam", "St. Johns", "St. Lucie", "Santa Rosa", "Sarasota", "Seminole", "Sumter",
        "Suwannee", "Taylor", "Union", "Volusia", "Wakulla", "Walton", "Washington"
    ];

    private static readonly IReadOnlyList<string> Majors =
    [
        "Accounting", "Biology", "Business", "Computer Science", "Education", "Engineering",
        "Health Sciences", "Nursing", "Psychology", "Undecided"
    ];

    private static readonly IReadOnlyList<string> SchoolPlaceholders =
    [
        "High school placeholder 1", "High school placeholder 2", "High school placeholder 3",
        "High school placeholder 4", "High school placeholder 5"
    ];

    private static readonly IReadOnlyList<string> SummaryTrends =
    [
        "Participation may vary over time; compare activity across reporting periods.",
        "Some groups may show higher engagement than others.",
        "Review these sample trends alongside the underlying data before drawing conclusions."
    ];

    public static DashboardViewModel Create(bool canEdit)
    {
        return new DashboardViewModel
        {
            CanEdit = canEdit,
            Sources =
            [
                new MetricSourceViewModel
                {
                    Name = "Coursera",
                    Id = "coursera",
                    FilterKind = DashboardFilterKind.Coursera,
                    CourseraSortOptions =
                    [
                        new() { Value = "student-name-ascending", Text = "Student Name (A–Z)", IsDefault = true },
                        new() { Value = "course-name-ascending", Text = "Course Name (A–Z)" },
                        new() { Value = "total-hours-ascending", Text = "Total Hours (Ascending)" },
                        new() { Value = "total-hours-descending", Text = "Total Hours (Descending)" },
                        new() { Value = "completion-ascending", Text = "Completion % (Ascending)" },
                        new() { Value = "completion-descending", Text = "Completion % (Descending)" }
                    ],
                    SummaryText = "This sample summary highlights the overall activity in the Coursera data. The available information suggests steady participation, with changes in engagement across the reporting period.",
                    SummaryTrends = SummaryTrends
                },
                new MetricSourceViewModel
                {
                    Name = "Google Forms",
                    Id = "google-forms",
                    FilterKind = DashboardFilterKind.GoogleForms,
                    MentorOptions = ["Mentor 1", "Mentor 2", "Mentor 3", "Mentor 4", "Mentor 5"],
                    CommunicationMethodOptions =
                    [
                        new() { Value = "text-message", Text = "Text message" },
                        new() { Value = "email", Text = "Email" },
                        new() { Value = "in-person", Text = "In-person" }
                    ],
                    SummaryText = "This sample summary highlights the overall activity in the Google Forms data. The available information suggests steady participation, with changes in engagement across the reporting period.",
                    SummaryTrends = SummaryTrends
                },
                new MetricSourceViewModel
                {
                    Name = "Cognito",
                    Id = "cognito",
                    FilterKind = DashboardFilterKind.Cognito,
                    FloridaCountyOptions = FloridaCounties,
                    SchoolOptions = SchoolPlaceholders,
                    MajorOptions = Majors,
                    SummaryText = "This sample summary highlights the overall activity in the Cognito data. The available information suggests steady participation, with changes in engagement across the reporting period.",
                    SummaryTrends = SummaryTrends
                }
            ]
        };
    }
}
