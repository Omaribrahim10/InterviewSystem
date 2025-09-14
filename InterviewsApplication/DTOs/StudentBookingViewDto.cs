namespace InterviewsApplication.DTOs
{
    public class StudentBookingViewDto
    {
        public int BookingID { get; set; }
        public string UniversityID { get; set; }
        public int ScheduleID { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime InterviewDate { get; set; }
    }
}
