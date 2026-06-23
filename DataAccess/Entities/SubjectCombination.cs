using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class SubjectCombination
    {
        [Key]
        public Guid CombinationId { get; set; }

        public string? Code { get; set; }
        public string? Subjects { get; set; }
    }
}
