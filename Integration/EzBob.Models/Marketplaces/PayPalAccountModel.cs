using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountModel
    {
        public PaymentAccountsModel GeneralInfo { get; set; }
        public PayPalAccountInfoModel PersonalInfo { get; set; }
    }
}