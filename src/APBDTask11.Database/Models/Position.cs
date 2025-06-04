namespace APBDTask11.Database.Models;

public class Position
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MinExpYears { get; set; }

    public ICollection<Employee> Employees { get; set; }
}