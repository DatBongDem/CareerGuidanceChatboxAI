using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class MajorTrait
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Major")]
        public Guid MajorId { get; set; }

        [ForeignKey("Trait")]
        public Guid TraitId { get; set; }

        public double Weight { get; set; }

        public Major? Major { get; set; }
        public Trait? Trait { get; set; }
    }
}
