using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay
{
    /// <summary>
    /// DTO for MP_EbayUserAccountData
    /// </summary>
    public class EbayUserAccountData
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public string PaymentMethod { get; set; }
        public bool? PastDue { get; set; }
        public double? CurrentBalance { get; set; }
        public DateTime? CreditCardModifyDate { get; set; }
        public string CreditCardInfo { get; set; }
        public DateTime? CreditCardExpiration { get; set; }
        public DateTime? BankModifyDate { get; set; }
        public string AccountState { get; set; }
        public string AmountPastDueCurrency { get; set; }
        public string BankAccountInfo { get; set; }
        public string AccountId { get; set; }
        public string Currency { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
