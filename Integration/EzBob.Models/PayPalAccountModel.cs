using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountModel
    {
        public PayPalModel GeneralInfo { get; set; }
        public PayPalAccountInfoModel PersonalInfo { get; set; }
        public PayPalAccountGeneralPaymentsInfoModel Payments { get; set; }
        public PayPalAccountDetailPaymentsInfoModel DetailPayments { get; set; }
    }
}