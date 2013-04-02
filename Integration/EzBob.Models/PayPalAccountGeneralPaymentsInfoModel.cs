using System.Collections.Generic;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountGeneralPaymentsInfoModel
    {
        public IEnumerable<PayPalGeneralDataRowModel> Data { get; set; }
    }
}