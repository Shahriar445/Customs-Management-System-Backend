namespace Customs_Management_System.DTOs
{
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; } 
        public string Status { get; set; }
    }
}
