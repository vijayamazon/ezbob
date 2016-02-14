namespace EzBobModels.Customer
{
    public enum CustomerPropertyStatus
    {
        NotYetFilled = 0,
        IOwnOnlyThisProperty = 1,
        IOwnThisPropertyAndOtherProperties,
        ILiveInTheAboveAndOwnOtherProperties,
        IHomeOwner,
        Renting,
        SocialHouse,
        LivingWithParents
    }
}
