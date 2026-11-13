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

    private static readonly IReadOnlyList<string> CognitoSchools =
    [
        "Miami Senior High School", "Coral Gables Senior High School", "Dillard High School",
        "William T. Dwyer High School", "Edgewater High School", "Plant High School",
        "Seminole High School", "Buchholz High School"
    ];

    private static readonly IReadOnlyList<string> SummaryTrends =
    [
        "Participation may vary over time; compare activity across reporting periods.",
        "Some groups may show higher engagement than others.",
        "Review these sample trends alongside the underlying data before drawing conclusions."
    ];

    private static IReadOnlyList<IReadOnlyList<string>> Rows(params string[][] rows) => rows;

    public static DashboardViewModel Create(bool canEdit)
    {
        return new DashboardViewModel
        {
            CanEdit = canEdit,
            Sources =
            [
                CreateCourseraSource(),
                CreateGoogleFormsSource(),
                CreateCognitoSource()
            ]
        };
    }

    private static MetricSourceViewModel CreateCourseraSource()
    {
        return new MetricSourceViewModel
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
            SummaryTrends = SummaryTrends,
            Table = new DashboardTableViewModel
            {
                Columns = ["Student Name", "Course Name/ID", "Hours Spent", "Completion %"],
                Rows = Rows(
                    ["Alice Johnson", "CS101", "12.5", "85%"],
                    ["Alice Johnson", "CS205", "8", "72%"],
                    ["Marcus Lee", "BIO110", "15", "95%"],
                    ["Priya Patel", "MATH201", "10.5", "60%"],
                    ["Diego Ramirez", "ENG101", "6.5", "40%"],
                    ["Sofia Chen", "CS101", "14", "88%"],
                    ["Jamal Wright", "NUR120", "9", "75%"],
                    ["Hannah Kim", "PSY101", "11", "100%"])
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "coursera-completion-chart",
                    Title = "Completion Percentage by Student and Course",
                    Type = "bar",
                    Labels =
                    [
                        "Alice Johnson", "Alice Johnson", "Marcus Lee", "Priya Patel",
                        "Diego Ramirez", "Sofia Chen", "Jamal Wright", "Hannah Kim"
                    ],
                    SubLabels =
                    [
                        "CS101", "CS205", "BIO110", "MATH201",
                        "ENG101", "CS101", "NUR120", "PSY101"
                    ],
                    MaxValue = 100m,
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Completion %",
                            Values = [85m, 72m, 95m, 60m, 40m, 88m, 75m, 100m]
                        }
                    ]
                }
            ]
        };
    }

    private static MetricSourceViewModel CreateGoogleFormsSource()
    {
        return new MetricSourceViewModel
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
            SummaryTrends = SummaryTrends,
            Table = new DashboardTableViewModel
            {
                Columns = ["Mentee Name", "Mentor Name", "Communication Method", "Additional Notes"],
                TruncateColumnIndex = 3,
                Rows = Rows(
                    ["Alex Turner", "Mentor 1", "Text message", "Prefers weekly check-ins."],
                    ["Alex Turner", "Mentor 1", "Email", "Requested scholarship resources."],
                    ["Alex Turner", "Mentor 1", "In-person", "Met at the fall kickoff event."],
                    ["Bianca Ruiz", "Mentor 2", "Text message", "Needs help with the FAFSA deadline."],
                    ["Bianca Ruiz", "Mentor 2", "Email", "Shared the college application checklist."],
                    ["Carlos Mendez", "Mentor 1", "In-person", "Interested in engineering programs and asked about internship opportunities, campus visits, and scholarship deadlines for the upcoming academic year."],
                    ["Carlos Mendez", "Mentor 1", "Text message", "Confirmed the upcoming campus visit."],
                    ["Dana Whitfield", "Mentor 3", "Email", "Discussing dual enrollment options."],
                    ["Eli Brooks", "Mentor 2", "Text message", "Check-in about course selection."],
                    ["Fatima Noor", "Mentor 3", "In-person", "Reviewed graduation requirements."])
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "google-forms-communication-chart",
                    Title = "Communication Methods Used",
                    Type = "pie",
                    Labels = ["Text message", "Email", "In-person"],
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Total",
                            Values = [4m, 3m, 3m]
                        }
                    ]
                }
            ]
        };
    }

    private static MetricSourceViewModel CreateCognitoSource()
    {
        return new MetricSourceViewModel
        {
            Name = "Cognito",
            Id = "cognito",
            FilterKind = DashboardFilterKind.Cognito,
            FloridaCountyOptions = FloridaCounties,
            SchoolOptions = CognitoSchools,
            MajorOptions = Majors,
            SummaryText = "This sample summary highlights the overall activity in the Cognito data. The available information suggests steady participation, with changes in engagement across the reporting period.",
            SummaryTrends = SummaryTrends,
            Table = new DashboardTableViewModel
            {
                Columns = ["Applicant Name", "School Location", "Personal Location", "Major of Choice"],
                Rows = Rows(
                    ["Aaliyah Brooks", "Miami Senior High School, Miami", "Miami-Dade", "Biology"],
                    ["Daniel Ortega", "Coral Gables Senior High School, Coral Gables", "Miami-Dade", "Engineering"],
                    ["Maya Thompson", "Dillard High School, Fort Lauderdale", "Broward", "Nursing"],
                    ["Elijah Grant", "William T. Dwyer High School, Palm Beach Gardens", "Palm Beach", "Business"],
                    ["Sofia Ramirez", "Edgewater High School, Orlando", "Orange", "Psychology"],
                    ["Noah Bennett", "Plant High School, Tampa", "Hillsborough", "Computer Science"],
                    ["Layla Hassan", "Seminole High School, Sanford", "Seminole", "Biology"],
                    ["Caleb Foster", "Buchholz High School, Gainesville", "Alachua", "Undecided"])
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "cognito-major-chart",
                    Title = "Applicants by Major of Choice",
                    Type = "bar",
                    Labels =
                    [
                        "Biology", "Business", "Computer Science", "Engineering",
                        "Nursing", "Psychology", "Undecided"
                    ],
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Applicants",
                            Values = [2m, 1m, 1m, 1m, 1m, 1m, 1m]
                        }
                    ]
                }
            ]
        };
    }
}
