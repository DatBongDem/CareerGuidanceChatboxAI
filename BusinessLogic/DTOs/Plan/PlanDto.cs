namespace BusinessLogic.DTOs.Plan
{
    public class PlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int UsersCount { get; set; }
    }
}