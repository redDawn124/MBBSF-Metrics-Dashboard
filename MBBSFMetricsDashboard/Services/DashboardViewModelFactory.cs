using System.Globalization;
using MBBSFMetricsDashboard.Models;

namespace MBBSFMetricsDashboard.Services;

public static class DashboardViewModelFactory
{
    // Temporary load-test fixture: pad every table to this many rows so the
    // dashboard can be exercised with a large dataset. Set to 0 to keep only
    // the original sample rows.
    private const int LoadTestRowTarget = 75;

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

    private static readonly string[] Majors =
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

    private static IReadOnlyList<IReadOnlyList<string>> PadWithGeneratedRows(
        IReadOnlyList<IReadOnlyList<string>> seedRows,
        Func<int, string[]> createGeneratedRow)
    {
        var rows = seedRows.ToList();
        for (var index = 0; rows.Count < LoadTestRowTarget; index++)
        {
            rows.Add(createGeneratedRow(index));
        }

        return rows;
    }

    private static IReadOnlyList<(string Label, decimal Count)> Tally(
        IReadOnlyList<IReadOnlyList<string>> rows,
        int columnIndex) =>
        rows.GroupBy(row => row[columnIndex])
            .Select(group => (Label: group.Key, Count: (decimal)group.Count()))
            .ToList();

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
        var seedRows = Rows(
            ["Alice Johnson", "CS101", "12.5", "85%"],
            ["Alice Johnson", "CS205", "8", "72%"],
            ["Marcus Lee", "BIO110", "15", "95%"],
            ["Priya Patel", "MATH201", "10.5", "60%"],
            ["Diego Ramirez", "ENG101", "6.5", "40%"],
            ["Sofia Chen", "CS101", "14", "88%"],
            ["Jamal Wright", "NUR120", "9", "75%"],
            ["Hannah Kim", "PSY101", "11", "100%"]);

        var courseraRows = PadWithGeneratedRows(seedRows, index =>
        {
            var seed = seedRows[index % seedRows.Count];
            var hours = 2m + (index % 19) + (index % 4) * 0.25m;
            var completion = 35 + (index * 7) % 66;
            return [seed[0], seed[1], hours.ToString("0.##", CultureInfo.InvariantCulture), $"{completion}%"];
        });

        var completionByStudentCourse = courseraRows
            .GroupBy(row => (Student: row[0], Course: row[1]))
            .Select(group => (
                Student: group.Key.Student,
                Course: group.Key.Course,
                Completion: group.Average(row => decimal.Parse(row[3].TrimEnd('%'), CultureInfo.InvariantCulture))))
            .ToList();

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
                Rows = courseraRows
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "coursera-completion-chart",
                    Title = "Completion Percentage by Student and Course",
                    Type = "bar",
                    Labels = completionByStudentCourse.Select(item => item.Student).ToList(),
                    SubLabels = completionByStudentCourse.Select(item => item.Course).ToList(),
                    MaxValue = 100m,
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Completion %",
                            Values = completionByStudentCourse.Select(item => item.Completion).ToList()
                        }
                    ]
                }
            ]
        };
    }

    private static MetricSourceViewModel CreateGoogleFormsSource()
    {
        var seedRows = Rows(
            ["Alex Turner", "Mentor 1", "Text message", "Prefers weekly check-ins."],
            ["Alex Turner", "Mentor 1", "Email", "Requested scholarship resources."],
            ["Alex Turner", "Mentor 1", "In-person", "Met at the fall kickoff event."],
            ["Bianca Ruiz", "Mentor 2", "Text message", "Needs help with the FAFSA deadline."],
            ["Bianca Ruiz", "Mentor 2", "Email", "Shared the college application checklist."],
            ["Carlos Mendez", "Mentor 1", "In-person", "Interested in engineering programs and asked about internship opportunities, campus visits, and scholarship deadlines for the upcoming academic year."],
            ["Carlos Mendez", "Mentor 1", "Text message", "Confirmed the upcoming campus visit."],
            ["Dana Whitfield", "Mentor 3", "Email", "Discussing dual enrollment options."],
            ["Eli Brooks", "Mentor 2", "Text message", "Check-in about course selection."],
            ["Fatima Noor", "Mentor 3", "In-person", "Reviewed graduation requirements."]);

        var contactRows = PadWithGeneratedRows(seedRows, index =>
        {
            var seed = seedRows[index % seedRows.Count];
            return [seed[0], seed[1], seed[2], $"Follow-up check-in {index + 1} (load-test data)."];
        });

        // Column 0 is the mentee name; each row represents one mentor contact.
        var contactsByStudent = Tally(contactRows, 0);
        var contactsByMethod = Tally(contactRows, 2);

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
                Rows = contactRows
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "google-forms-communication-chart",
                    Title = "Communication Methods Used",
                    Type = "pie",
                    Labels = contactsByMethod.Select(contact => contact.Label).ToList(),
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Total",
                            Values = contactsByMethod.Select(contact => contact.Count).ToList()
                        }
                    ]
                },
                new DashboardChartViewModel
                {
                    Id = "google-forms-mentor-contacts-chart",
                    Title = "Mentor Contacts by Student",
                    Type = "bar",
                    Labels = contactsByStudent.Select(contact => contact.Label).ToList(),
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Mentor contacts",
                            Values = contactsByStudent.Select(contact => contact.Count).ToList()
                        }
                    ]
                }
            ]
        };
    }

    private static MetricSourceViewModel CreateCognitoSource()
    {
        var seedRows = Rows(
            ["Aaliyah Brooks", "Miami Senior High School, Miami", "Miami-Dade", "Biology"],
            ["Daniel Ortega", "Coral Gables Senior High School, Coral Gables", "Miami-Dade", "Engineering"],
            ["Maya Thompson", "Dillard High School, Fort Lauderdale", "Broward", "Nursing"],
            ["Elijah Grant", "William T. Dwyer High School, Palm Beach Gardens", "Palm Beach", "Business"],
            ["Sofia Ramirez", "Edgewater High School, Orlando", "Orange", "Psychology"],
            ["Noah Bennett", "Plant High School, Tampa", "Hillsborough", "Computer Science"],
            ["Layla Hassan", "Seminole High School, Sanford", "Seminole", "Biology"],
            ["Caleb Foster", "Buchholz High School, Gainesville", "Alachua", "Undecided"]);

        var applicantRows = PadWithGeneratedRows(seedRows, index =>
        {
            var seed = seedRows[index % seedRows.Count];
            return [$"Test Applicant {index + 1}", seed[1], seed[2], seed[3]];
        });

        // Column 2 is the applicant's personal location (county).
        var applicantsByCounty = Tally(applicantRows, 2);
        var applicantsByMajor = Tally(applicantRows, 3)
            .OrderBy(applicant => Majors.IndexOf(applicant.Label))
            .ToList();

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
                Rows = applicantRows
            },
            Charts =
            [
                new DashboardChartViewModel
                {
                    Id = "cognito-major-chart",
                    Title = "Applicants by Major of Choice",
                    Type = "bar",
                    Labels = applicantsByMajor.Select(applicant => applicant.Label).ToList(),
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Applicants",
                            Values = applicantsByMajor.Select(applicant => applicant.Count).ToList()
                        }
                    ]
                },
                new DashboardChartViewModel
                {
                    Id = "cognito-county-chart",
                    Title = "Applicants by County",
                    Type = "bar",
                    Labels = applicantsByCounty.Select(county => county.Label).ToList(),
                    Series =
                    [
                        new DashboardSeriesViewModel
                        {
                            Label = "Applicants",
                            Values = applicantsByCounty.Select(county => county.Count).ToList()
                        }
                    ]
                }
            ]
        };
    }
}
