using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountDetailPaymentsInfoModel
    {
        public IEnumerable<PayPalGeneralDataRowModel> Income { get; set; }
        public IEnumerable<PayPalGeneralDataRowModel> Expenses { get; set; }
    }
}