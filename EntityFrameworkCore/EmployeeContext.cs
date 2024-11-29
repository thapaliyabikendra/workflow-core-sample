using ACMS.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACMS.WebApi.EntityFrameworkCore;

public class EmployeeContext : DbContext
{
    public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
}