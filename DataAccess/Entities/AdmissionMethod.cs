using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Entities
{
    public class AdmissionMethod
    {
        [Key]
        public Guid MethodId { get; set; }

        public string Name { get; set; }


        [JsonIgnore]
        public ICollection<UniversityMajorMethod>? UniversityMajorMethods { get; set; }

    }
}