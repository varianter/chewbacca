namespace Invoicing.Models
{
    public partial class EntryModel
    {
        public List<TimeEntry> time_entries { get; set; }

        public long per_page { get; set; }

        public long total_pages { get; set; }

        public long total_entries { get; set; }

        public object next_page { get; set; }

        public object previous_page { get; set; }

        public long page { get; set; }

        public Links links { get; set; }
    }


    public partial class TimeEntry
    {
        public long id { get; set; }

        public DateTimeOffset spent_date { get; set; }

        public double hours { get; set; }

        public User user { get; set; }
    }

    public partial class User
    {
        public long id { get; set; }

        public string name { get; set; }

    }


    public partial class Links
    {

        public object next { get; set; }

    }
}

