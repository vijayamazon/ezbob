namespace EzBobModels.Hmrc
{
    /// <summary>
    /// This class also used for ChannelGraber.
    /// It should be exactly in this form. The name of the class should not be changed. 
    /// Casing of properties is important, otherwise it would not be compatible with already existing data in DB
    /// </summary>
    public class AccountModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public int limitDays { get; set; }
        public string auxLogin { get; set; }
        public string auxPassword { get; set; }
        public int realmId { get; set; }
        public string accountTypeName { get; set; }

        public string displayName { get; set; }
    }
}
