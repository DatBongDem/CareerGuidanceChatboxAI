using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class UniversityMajorMethod
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("University")]
        public Guid UniversityId { get; set; }

        [ForeignKey("Major")]
        public Guid MajorId { get; set; }

        [ForeignKey("AdmissionMethod")]
        public Guid MethodId { get; set; }

        public string? Note { get; set; }

        public University? University { get; set; }
        public Major? Major { get; set; }

        public AdmissionMethod? AdmissionMethod { get; set; }
    }
}
