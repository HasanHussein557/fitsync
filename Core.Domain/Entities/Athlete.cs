using System.Collections.Generic;

namespace Core.Domain.Entities;

public class Athlete
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int Age { get; set; }

    public int Height { get; set; }

    public int Weight { get; set; }

    public string Sex { get; set; } = "Male";

    public string Goal { get; set; } = "Maintenance";

    // Get full name property
    public string Name
    {
        get { return $"{FirstName} {LastName}".Trim(); }
        set 
        {
            if (string.IsNullOrEmpty(value))
            {
                FirstName = string.Empty;
                LastName = string.Empty;
                return;
            }
            
            var parts = value.Trim().Split(' ');
            if (parts.Length == 1)
            {
                FirstName = parts[0];
                LastName = string.Empty;
            }
            else
            {
                FirstName = parts[0];
                LastName = string.Join(" ", parts, 1, parts.Length - 1);
            }
        }
    }
} 