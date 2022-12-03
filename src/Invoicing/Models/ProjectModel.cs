namespace Invoicing.Models
{
    public class ProjectList
    {
        public List<Project> projects { get; set; }
        public int per_page { get; set; }
        public int total_pages { get; set; }

        public int? next_page { get; set; }

    }
    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }

        public Client client { get; set; }

    }

    public class Client
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}