namespace EzBob3dParties.Yodlee.Models.Account
{
    class RewardActivity : YAccount
    {
        public int totalUnit { get; set; }
        
        public YMoney totalAmount { get; set; }
        
        public string memberName { get; set; }
    }
}
