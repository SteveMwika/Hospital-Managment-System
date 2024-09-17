namespace Hospital_Managment_System.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime appointmentDate { get; set; }
        public string status { get; set; }

        public int patientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

    }
}
