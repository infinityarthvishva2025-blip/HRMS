using System;
using HRMS.Models;

namespace HRMS.Services
{
    /// <summary>
    /// Attendance-related helpers (late calculation, etc.).
    /// </summary>
    public class AttendanceService
    {
        /// <summary>
        /// Sets IsLate and LateMinutes on a single Attendance record.
        /// Call this when you create/update attendance for a day.
        /// </summary>
        public void ProcessAttendance(Attendance a)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));

            a.IsLate = false;
            a.LateMinutes = 0;

            // Only apply late logic for present days with InTime
            bool presentType =
                a.Status == "P" ||
                a.Status == "WOP" ||
                a.Status == "½P" ||
                a.Status == "P½";

            if (!presentType || !a.InTime.HasValue)
                return;

            var day = a.Date.DayOfWeek;

            // Sunday is weekly off, ignore late logic
            if (day == DayOfWeek.Sunday)
                return;

            // Cut-off time (you can adjust):
            // Saturday: 10:15 AM
            // Mon–Fri:  9:45 AM
            TimeSpan cutoff =
                day == DayOfWeek.Saturday
                    ? new TimeSpan(10, 15, 0)
                    : new TimeSpan(9, 45, 0);

            if (a.InTime.Value > cutoff)
            {
                a.IsLate = true;
                a.LateMinutes = (int)(a.InTime.Value - cutoff).TotalMinutes;
            }
        }

        /// <summary>
        /// Convenience method used in some views: returns true/false for "Is this row late?"
        /// </summary>
        public bool IsLate(Attendance a)
        {
            if (a == null) return false;

            // If IsLate is already filled from DB, just return it
            if (a.IsLate) return true;

            // Otherwise recompute in-memory (does not save to DB)
            bool originalIsLate = a.IsLate;
            int originalLateMinutes = a.LateMinutes;

            ProcessAttendance(a);

            bool result = a.IsLate;

            // Restore original values to avoid side-effects
            a.IsLate = originalIsLate;
            a.LateMinutes = originalLateMinutes;

            return result;
        }
    }
}
