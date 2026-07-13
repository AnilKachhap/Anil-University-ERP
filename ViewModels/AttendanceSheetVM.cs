using System.ComponentModel.DataAnnotations;

namespace AnilUniversity.ViewModels
{
    public class AttendanceSheetVM
    {
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public List<AttendanceVM> Students { get; set; } = new();
    }
}