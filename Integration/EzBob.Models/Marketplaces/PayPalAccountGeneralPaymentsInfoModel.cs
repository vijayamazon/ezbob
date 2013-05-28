using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountGeneralPaymentsInfoModel
    {
        public IEnumerable<PayPalGeneralDataRowModel> Data { get; set; }
    }
}