namespace EzBobModels.Company
{
    using System;
    using EzBobModels.Enums;

    public class Director
    {
        public int? CustomerId { get; set; }
        public int CompanyId { get; set; }
        public int? id { get; set; }
        public string Name { get; set; }
        public string Middle { get; set; }
        public string Surname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool? IsShareholder { get; set; }
        public bool? IsDirector { get; set; }
        public int? ExperianConsumerScore { get; set; }
    }
}
