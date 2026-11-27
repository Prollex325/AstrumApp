using System;
using System.Collections.Generic;
using System.Text;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace AstrumApp.Models
{
    [Table("announcements")]
    public class Announcement : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("image_path")]
        public string? ImagePath { get; set; }

        [Column("category")]
        public string Category { get; set; } = "news";

        [Column("published_at")]
        public DateTime PublishedAt { get; set; }

        [Column("action_text")]
        public string? ActionText { get; set; }

        [Column("action_type")]
        public string ActionType { get; set; } = "none";

        [Column("action_parameter")]
        public string? ActionParameter { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_pinned")]
        public bool IsPinned { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }
    }
}
