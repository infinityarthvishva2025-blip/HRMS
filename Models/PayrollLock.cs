using System;
using System.ComponentModel.DataAnnotations;

public class PayrollLock
{
    [Key]
    public int Id { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }

    public bool IsLocked { get; set; }

    public string LockedBy { get; set; }

    public DateTime LockedOn { get; set; } = DateTime.Now;
}
