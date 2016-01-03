namespace EzBob3dParties.Yodlee.Models.Account
{
    class Bill : YAccount
    {
        public YDate paymDate { get; set; }
        
        public string acctType { get; set; }
        
        public YMoney payment { get; set; }

        public YDate paymentDate { get; set; }
        
        public string paymFreqCode { get; set; }
    }
}
