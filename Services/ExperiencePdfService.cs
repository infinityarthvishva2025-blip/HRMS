using HRMS.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HRMS.Services
{
    public class ExperiencePdfService
    {
        public byte[] GenerateExperienceLetter(Employee employee)
        {
            var today = DateTime.Today.ToString("dd/MM/yyyy");

            var heShe = employee.Gender == "Female" ? "She" : "He";
            var hisHer = employee.Gender == "Female" ? "her" : "his";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);

                    // Letterhead Background
                    page.Background()
                        .Image("wwwroot/images/letterhead.png")
                        .FitArea();

                    page.Content()
                        .PaddingTop(200)
                        .PaddingLeft(80)
                        .PaddingRight(80)
                        .PaddingBottom(150)
                        .Column(col =>
                        {
                            col.Spacing(10);

                            // Title
                            col.Item().AlignCenter().Text("Experience Letter")
                                .FontSize(18)
                                .Bold()
                                .Underline();

                            // Date
                            col.Item().AlignRight().Text($"Date: {today}")
                                .FontSize(11);

                            col.Item().PaddingTop(10)
                                .Text("To whomsoever it may concern,")
                                .FontSize(11);

                            // Paragraph 1
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("This is to certify that ");
                                text.Span(employee.Name).Bold();
                                text.Span(" was employed with ");
                                text.Span("Infinity Arthvishva").Bold();
                                text.Span(" from ");
                                text.Span(employee.JoiningDate?.ToString("dd/MM/yyyy")).Bold();
                                text.Span(" to ");
                                text.Span(employee.LastWorkingDate?.ToString("dd/MM/yyyy")).Bold();
                                text.Span(" as a ");
                                text.Span(employee.Position).Bold();
                                text.Span(" in the ");
                                text.Span(employee.Department).Bold();
                                text.Span(" department.");
                            });

                            // Paragraph 2
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("During the tenure of ");
                                text.Span(hisHer).Bold();
                                text.Span(" employment, ");
                                text.Span(heShe).Bold();
                                text.Span(" was responsible for assigned roles and responsibilities and performed duties sincerely and to the best of ");
                                text.Span(hisHer).Bold();
                                text.Span(" abilities.");
                            });

                            // Paragraph 3
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("Throughout ");
                                text.Span(hisHer).Bold();
                                text.Span(" association with the organization, ");
                                text.Span(employee.Name).Bold();
                                text.Span(" performed duties with sincerity, professionalism and integrity. ");
                                text.Span(heShe).Bold();
                                text.Span(" maintained proper conduct and discipline during the tenure of employment.");
                            });

                            // Paragraph 4
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("We appreciate ");
                                text.Span(hisHer).Bold();
                                text.Span(" contribution to the organization and wish ");
                                text.Span(hisHer).Bold();
                                text.Span(" success in future professional endeavors.");
                            });

                            // Regards
                            col.Item().PaddingTop(25).Text("Regards,")
                                .FontSize(11);

                            col.Item().Text("Infinity Arthvishva")
                                .FontSize(11);

                            // HR Signature
                            col.Item().PaddingTop(20)
                                .Height(60);
                               // .Image("wwwroot/images/hrsignature.png");

                            col.Item().Text("HR Department")
                                .FontSize(11);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}