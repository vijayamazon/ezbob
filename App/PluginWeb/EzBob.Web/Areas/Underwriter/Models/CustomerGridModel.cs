using System.Web.Mvc;
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
        public CustomerGridModel WaitingForDecision { get; set; }
        public CustomerGridModel Escalated { get; set; }
        public CustomerGridModel Approved { get; set; }
        public CustomerGridModel Rejected { get; set; }
        public CustomerGridModel All { get; set; }
        public CustomerGridModel RegisteredCustomers { get; set; }
        public CustomerGridModel Late { get; set; }
        public IEzBobConfiguration Config { get; set; }
        public decimal MaxLoan { get; set; }
    }
}