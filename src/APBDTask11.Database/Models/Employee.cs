namespace APBDTask11.Database.Models;

public class Employee
{
    public int Id { get; set; }
    public decimal Salary { get; set; }
    public int PositionId { get; set; }
    public int PersonId { get; set; }
    public DateTime HireDate { get; set; }

    public Position Position { get; set; }
    public Person Person { get; set; }
    public ICollection<DeviceEmployee> DeviceEmployees { get; set; }
    public ICollection<Account> Accounts { get; set; }
}