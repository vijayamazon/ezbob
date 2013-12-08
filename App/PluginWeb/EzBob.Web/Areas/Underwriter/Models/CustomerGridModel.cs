using System.Collections.Generic;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Infrastructure;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class CustomerGridModel
    {
        public MvcHtmlString ColModel { get; set; }
        public MvcHtmlString ColNames { get; set; }
        public string Action { get; set; }
    }

    public class LoansGrids
    {
	    public bool IsEscalated { get; set; }
	    public IEzBobConfiguration Config { get; set; }
        public decimal MaxLoan { get; set; }
        public IEnumerable<MP_MarketplaceType> MpTypes { get; set; }
        public List<CustomerStatuses> CollectionStatuses { get; set; }
    }
}