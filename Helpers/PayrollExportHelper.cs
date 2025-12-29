using ClosedXML.Excel;
using HRMS.Data;
using HRMS.Models;
using System.Collections.Generic;
using System.IO;

namespace HRMS.Helpers
{
    public static class PayrollExportHelper
    {
        public static byte[] ExportToExcel(List<Payroll> payrolls)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Payroll");

            // Header
            worksheet.Cell(1, 1).Value = "Emp Code";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Month";
            worksheet.Cell(1, 4).Value = "Year";
            worksheet.Cell(1, 5).Value = "Paid Days";
            worksheet.Cell(1, 6).Value = "Gross Salary";
            worksheet.Cell(1, 7).Value = "Deductions";
            worksheet.Cell(1, 8).Value = "Net Salary";

            int row = 2;
            foreach (var p in payrolls)
            {
                worksheet.Cell(row, 1).Value = p.emp_code;
                worksheet.Cell(row, 2).Value = p.name;
                worksheet.Cell(row, 3).Value = p.month;
                worksheet.Cell(row, 4).Value = p.year;
                worksheet.Cell(row, 5).Value = p.paid_days;
                worksheet.Cell(row, 6).Value = p.gross_salary;
                worksheet.Cell(row, 7).Value = p.total_deduction;
                worksheet.Cell(row, 8).Value = p.net_salary;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
