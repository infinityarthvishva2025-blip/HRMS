using System;

namespace HRMS.Models
{
    public class GurukulVideo
    {
        public int Id { get; set; }
        public string TitleGroup { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        // VideoPath: if IsExternal = false => relative path like "/uploads/gurukul/abcd.mp4"
        // if IsExternal = true => external URL (YouTube/Vimeo/Drive embed link)
        public string VideoPath { get; set; } = "";
        public bool IsExternal { get; set; } = false;
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

        public string? ThumbnailPath { get; set; }
    }
}
