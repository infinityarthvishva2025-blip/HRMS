using HRMS.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HRMS.Services
{
    public class QuestPdfService : IPdfService
    {
        public byte[] GenerateRelievingLetterPdf(Employee employee)
        {
            var today = DateTime.Today.ToString("dd/MM/yyyy");
            var lastWorkingDate = employee.LastWorkingDate?.ToString("dd/MM/yyyy") ?? "";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);

                    // LETTERHEAD BACKGROUND
                    page.Background()
                        .Image("wwwroot/images/letterhead.png")
                        .FitArea();

                    page.Content()
                        .PaddingTop(200)
                        .PaddingLeft(80)
                        .PaddingRight(80)
                        .PaddingBottom(140)
                        .Column(col =>
                        {
                            col.Spacing(12);

                            // Title
                            col.Item().AlignCenter().Text("Relieving Letter")
                                .FontSize(18)
                                .Bold()
                                .Underline();

                            // Date
                            col.Item().AlignRight().Text($"Date: {today}")
                                .FontSize(11);

                            col.Item().PaddingTop(20);

                            // Opening
                            col.Item().Text("To whomsoever it may concern,")
                                .FontSize(11);

                            // Paragraph 1
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("This is to inform you that ");
                                text.Span(employee.Name).Bold();
                                text.Span(" (");
                                text.Span(employee.Position).Bold();
                                text.Span("), has been relieved from the services of ");
                                text.Span("Infinity Arthvishva").Bold();
                                text.Span(" with effect from ");
                                text.Span(lastWorkingDate).Bold();
                                text.Span(", pursuant to his/her resignation.");
                            });

                            // Paragraph 2
                            col.Item().PaddingTop(10)
                                .Text("He/She completed all the assigned responsibilities and fulfilled the required exit formalities.")
                                .FontSize(11);

                            // Paragraph 3
                            col.Item()
                                .Text("There are no dues pending against him/her as on the last working day.")
                                .FontSize(11);

                            // Paragraph 4
                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(11));

                                text.Span("We thank ");
                                text.Span(employee.Name).Bold();
                                text.Span(" for his/her contributions to the organization during the tenure and wish him/her success in future endeavors.");
                            });

                            col.Item().PaddingTop(30);

                            // Regards
                            col.Item().Text("Regards,")
                                .FontSize(11);

                            col.Item().Text("Infinity Arthvishva")
                                .FontSize(11);

                            // Signature
                            col.Item().PaddingTop(20)
                                .Height(60);
                                //.Image("wwwroot/images/hrsignature.png");

                            col.Item().Text("HR Department")
                                .FontSize(11);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}