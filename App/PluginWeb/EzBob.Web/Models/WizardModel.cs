using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Infrastructure;

namespace EzBob.Web.Models
{
    public class WizardModel
    {
        public CustomerModel Customer { get; set; }
        public IEzBobConfiguration Config { get; set; }
    }
}