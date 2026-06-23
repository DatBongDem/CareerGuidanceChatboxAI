using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class Campus
    {
        [Key]
        public Guid CampusId { get; set; }

        [ForeignKey("University")]
        public Guid UniversityId { get; set; }

        public string? Name { get; set; }
        public string? Address { get; set; }

        public University? University { get; set; }
    }
}