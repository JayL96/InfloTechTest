using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;
public enum LogAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Viewed = 4
}

public class LogEntry
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    public LogAction Action { get; set; }

    [StringLength(500)]
    public string? Details { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
