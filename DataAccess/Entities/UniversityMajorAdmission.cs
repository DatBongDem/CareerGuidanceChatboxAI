using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class UniversityMajorAdmission
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("University")]
        public Guid UniversityId { get; set; }

        [ForeignKey("Major")]
        public Guid MajorId { get; set; }

        [ForeignKey("Campus")]
        public Guid? CampusId { get; set; }

        [ForeignKey("AdmissionMethod")]
        public Guid MethodId { get; set; }

        [ForeignKey("SubjectCombination")]
        public Guid CombinationId { get; set; }

        public int Year { get; set; }

        public double? CutoffScore { get; set; }
        public int? Quota { get; set; }
        public string? Note { get; set; }

        public University? University { get; set; }
        public Major? Major { get; set; }
        public Campus? Campus { get; set; }
        public AdmissionMethod? AdmissionMethod { get; set; }
        public SubjectCombination? SubjectCombination { get; set; }
    }
}
