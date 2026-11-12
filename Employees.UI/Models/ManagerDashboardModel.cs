namespace Employees.UI.Models
{
    public class ManagerDashboardModel
    {
        public int TotalHR { get; set; }

        public int TotalTechnical { get; set; }

        public int TotalCreated { get; set; }

        public List<DashboardUserModel> HRUsers { get; set; }
            = new();

        public List<DashboardUserModel> TechnicalUsers { get; set; }
            = new();
    }

    public class DashboardUserModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
