using System.Collections.Generic;
using HRMS.Models; // your model namespace

namespace HRMS.Repositories
{
    public interface IAttendanceRepository
    {
        IEnumerable<AttendanceRecord> GetAll();   // ✅ NO BODY
        void Add(AttendanceRecord record);        // ✅ NO BODY
    }
}

//using HRMS.Models;

//namespace HRMS.Repositories
//{
//    public class IAttendanceRepository
//    {

//        //IEnumerable<AttendanceRecord> GetAll();
//        //void Add(AttendanceRecord record);
//        IEnumerable<AttendanceRecord> GetAll()
//        {
//            // ❌ WRONG — method body not allowed in interfaces
//            return null;
//        }
//    }
//}
