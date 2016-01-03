namespace EzBob3dParties.Yodlee.Models.Account
{
    class Holding : YAccount
    {
        public string holdingType { get; set; }
        
        public string symbol { get; set; }
        
        public string cusipNumber { get; set; }
        
        public YMoney price { get; set; }
        
        public YMoney value { get; set; }
        
        public string description { get; set; }
    }
}
