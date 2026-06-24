using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Trait
    {
        [Key]
        public Guid TraitId { get; set; }

        public string? Name { get; set; }
    }
}