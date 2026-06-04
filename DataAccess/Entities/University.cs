using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class University
    {
        [Key]
        public Guid UniversityId { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Location { get; set; }
        public double Ranking { get; set; }
        public string? Avatar { get; set; }
    }
}