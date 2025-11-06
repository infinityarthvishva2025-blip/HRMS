using System.Collections.Generic;
using HRMS.Models;

namespace HRMS.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly List<AttendanceRecord> _db = new();

        public IEnumerable<AttendanceRecord> GetAll()
        {
            return _db;   // ✅ method body is allowed here
        }

        public void Add(AttendanceRecord record)
        {
            record.Id = _db.Count + 1;
            _db.Add(record);
        }
    }
}

//using HRMS.Models;
//using HRMS.Repositories;

//namespace HRMS.Repositories
//{


//    public class AttendanceRepository : IAttendanceRepository
//    {
//        private readonly List<AttendanceRecord> _db = new();

//        public IEnumerable<AttendanceRecord> GetAll() => _db;

//        public void Add(AttendanceRecord record)
//        {
//            record.Id = _db.Count + 1;
//            _db.Add(record);
//        }
//    }
//}
