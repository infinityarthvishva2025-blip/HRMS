using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRMS.ViewModels
{
    public class DailyReportViewModel
    {
        [Required(ErrorMessage = "Today's Work is required")]
        public string TodaysWork { get; set; }

        public string PendingWork { get; set; }

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
