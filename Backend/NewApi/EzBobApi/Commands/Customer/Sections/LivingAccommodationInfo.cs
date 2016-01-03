namespace EzBobApi.Commands.Customer.Sections
{
    /// <summary>
    /// http://www.hmrc.gov.uk/manuals/eimanual/eim11321.htm
    /// </summary>
    public class LivingAccommodationInfo : LivingAddressInfo
    {
        public int HousingType { get; set; } // Renting, Social, Parents
    }
}
