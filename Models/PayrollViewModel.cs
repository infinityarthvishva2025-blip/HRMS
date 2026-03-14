using System;
using System.ComponentModel.DataAnnotations;

public class PayrollViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }
    //[Required]
    //[Display(Name = "From Date")]
    //public DateTime FromDate { get; set; }

    //[Required]
    //[Display(Name = "To Date")]
    //public DateTime ToDate { get; set; }

    //[Required]
    //public int Month { get; set; }

    //[Required]
    //public int Year { get; set; }
}
