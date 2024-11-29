using ACMS.WebApi.Entities;
using ACMS.WebApi.EntityFrameworkCore;
using Bogus;

namespace ACMS.WebApi.Utilities;

public static class DataSeeder
{
    public static void Seed(EmployeeContext dbContext)
    {
        // Check if the database is already seeded
        if (!dbContext.Employees.Any())
        {
            // Use Bogus to generate fake data
            var faker = new Faker<Employee>()
                .RuleFor(e => e.Name, f => f.Name.FullName())
                .RuleFor(e => e.Department, f => f.Commerce.Department())
                .RuleFor(e => e.Branch, f => f.PickRandom("Branch A", "Branch B", "Branch C"));

            var employees = faker.Generate(50);  // Generate 50 fake employees

            dbContext.Employees.AddRange(employees);
            dbContext.SaveChanges();  // Save the generated data to the database
        }
    }
}
