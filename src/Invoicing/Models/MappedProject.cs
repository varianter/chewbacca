namespace Invoicing.Models
{
    public class MappedProject

    {

        public Project project { get; set; }

        public string month { get; set; }
        public int year { get; set; }

        public int numberOfDays { get; set; }

        public List<UserHours> monthly_entries { get; set; }
        public DateTime initialDate { get; set; }

    }

    public class UserHours
    {
        public User user { get; set; }

        public List<double> parsed_hours { get; set; }
        public List<Hours> spent_hours_by_date { get; set; }

        public double sum { get; set; }
    }

    public class Hours
    {
        public DateTimeOffset spent_date { get; set; }
        public double spent_hours { get; set; }

    }
}