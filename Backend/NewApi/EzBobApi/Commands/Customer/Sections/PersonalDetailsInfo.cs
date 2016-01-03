namespace EzBobApi.Commands.Customer.Sections {
    using System;
    using EzBobModels.Enums;

    /// <summary>
    /// Contains personal information related data
    /// </summary>
    public class PersonalDetailsInfo {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string SurName { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MaritalStatus { get; set; }
    }
}
