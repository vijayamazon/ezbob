namespace EzBob3dParties.Yodlee.Models.Account
{
    class RewardBalance : YAccount
    {
        public double balanceAmount { get; set; }

        public string balanceUnit { get; set; }
        
        public string memberName { get; set; }
    }
}
