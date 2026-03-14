using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRMS.ViewModels
{
    public class DailyReportViewModel
    {
        [Required(ErrorMessage = "⚠ Please enter today's work details.")]
        public string TodaysWork { get; set; }
        [Required(ErrorMessage = "⚠ Please mention pending work (enter 'None' if not applicable).")]
        public string PendingWork { get; set; }
        [Required(ErrorMessage = "⚠ Please describe issues or challenges (enter 'None' if not applicable).")]
        public string Issues { get; set; }

        // ✅ OPTIONAL attachment
        public IFormFile? Attachment { get; set; }

        // ✅ Checkbox binding-safe
        public int[] SelectedRecipientIds { get; set; } = new int[0];

        // ✅ Always initialized
        public IEnumerable<SelectListItem> RecipientList { get; set; }
            = new List<SelectListItem>();
    }
}
