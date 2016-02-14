namespace EzBobModels.Enums
{
    using System.ComponentModel;

    public enum CustomerAddressType
    {
        [Description("Personal")]
        PersonalAddress = 1,
        [Description("Previous Personal")]
        PrevPersonAddresses = 2,
        [Description("Company (lim)")]
        LimitedCompanyAddress = 3,
        [Description("Director (lim)")]
        LimitedDirectorHomeAddress = 4,
        [Description("Company (non lim)")]
        NonLimitedCompanyAddress = 5,
        [Description("Director (non lim)")]
        NonLimitedDirectorHomeAddress = 6,
        [Description("Previous Company (lim)")]
        LimitedCompanyAddressPrev = 7,
        [Description("Previous Director (lim)")]
        LimitedDirectorHomeAddressPrev = 8,
        [Description("Previous Company (non lim)")]
        NonLimitedCompanyAddressPrev = 9,
        [Description("Previous Director (non lim)")]
        NonLimitedDirectorHomeAddressPrev = 10,
        [Description("Other")]
        OtherPropertyAddress = 11,
        [Description("Other (old)")]
        OtherPropertyAddressRemoved = 12, // Was 11 in the past. 'No longer owned property'
        [Description("Experian Company")]
        ExperianCompanyAddress = 13
    }
}
