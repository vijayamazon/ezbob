namespace EzBobModels.Customer {
    using System;

    public class CustomerDetails {
        public int Id { get; set; }
        public int RequestedId { get; set; }

        public bool StatusIsEnabled { get; set; }
        public bool StatusIsWarning { get; set; }
        public string StatusName { get; set; }
        public bool IsOffline { get; set; }
        public bool IsTest { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public bool IsOwnerOfMainAddress { get; set; }
        public bool IsOwnerOfOtherProperties { get; set; }
        public string PropertyStatusDescription { get; set; }
        public int NumOfMps { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int NumOfLoans { get; set; }
        public int NumOfHmrcMps { get; set; }
        public bool IsAlibaba { get; set; }
        public int? BrokerId { get; set; }
        public int NumOfYodleeMps { get; set; }
        public DateTime? EarliestHmrcLastUpdateDate { get; set; }
        public DateTime? EarliestYodleeLastUpdateDate { get; set; }
        public bool FilledByBroker { get; set; }
        public int NumOfPreviousApprovals { get; set; }
        public string FullName { get; set; }

        public int ExperianConsumerScore { get; set; }

        public bool OwnsProperty
        {
            get { return IsOwnerOfMainAddress || IsOwnerOfOtherProperties; }
        }

        public bool IsValid
        {
            get { return (Id > 0) && (Id == RequestedId); }
        }
    }
}
