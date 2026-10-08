namespace Employee.Data.Models
{
    public class Employee1
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Department { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public int? UserId { get; set; }


        public int? ManagerId { get; set; }

        public string Team { get; set; } = string.Empty;

        public User? User { get; set; }

        public User? Manager { get; set; }
    }
}

